using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DomainDrivenDelivery.Application.Handling;

using Dotnet.Commons.Logging;

using Quartz;

using Spring.Objects.Factory;
using Spring.Scheduling.Quartz;

namespace DomainDrivenDelivery.Interfaces.Handling.Files
{
    /// <summary>
    /// Periodically scans a certain directory for files and attempts
    /// to parse handling event registrations from the contents.
    /// </summary>
    /// <remarks>
    /// Files that fail to parse are moved into a separate directory,
    /// successful files are deleted.
    /// </remarks>
    public class UploadDirectoryScanner : QuartzJobObject, IInitializingObject
    {
        private DirectoryInfo uploadDirectory;
        private DirectoryInfo parseFailureDirectory;
        private HandlingEventService handlingEventService;

        private readonly static ILog logger = LogFactory.GetLogger(typeof(UploadDirectoryScanner));

        protected override void ExecuteInternal(JobExecutionContext context)
        {
            foreach(FileInfo file in uploadDirectory.GetFiles())
            {
                try
                {
                    parse(file);
                    delete(file);
                    logger.Info("Import of " + file.Name + " complete");
                }

                catch(Exception e)
                {
                    logger.Error(e, e);
                    move(file);
                }
            }
        }

        private void parse(FileInfo file)
        {
            var lines = File.ReadAllLines(file.FullName);
            var rejectedLines = new List<string>();
            foreach(string line in lines)
            {
                try
                {
                    parseLine(line);
                }

                catch(Exception e)
                {
                    logger.Error("Rejected line \n" + line + "\nReason is: " + e);
                    rejectedLines.Add(line);
                }
            }
            if(rejectedLines.Any())
            {
                writeRejectedLinesToFile(toRejectedFilename(file), rejectedLines);
            }
        }

        private string toRejectedFilename(FileInfo file)
        {
            return file.Name + ".reject";
        }

        private void writeRejectedLinesToFile(string filename, List<string> rejectedLines)
        {
            File.WriteAllLines(Path.Combine(parseFailureDirectory.FullName, filename), rejectedLines);
        }

        private void parseLine(string line)
        {
            var columns = line.Split('\t');
            if(columns.Length == 5)
            {
                queueAttempt(columns[0], columns[1], columns[2], columns[3], columns[4]);
            }
            else if(columns.Length == 4)
            {
                queueAttempt(columns[0], columns[1], "", columns[2], columns[3]);
            }
            else
            {
                throw new FormatException("Wrong number of columns on line: " + line + ", must be 4 or 5");
            }
        }

        private void queueAttempt(string completionTimeStr, string trackingIdStr, string voyageNumberStr, string unLocodeStr, string eventTypeStr)
        {
            var errors = new List<string>();

            var date = HandlingReportParser.parseDate(completionTimeStr, errors);
            var trackingId = HandlingReportParser.parseTrackingId(trackingIdStr, errors);
            var voyageNumber = HandlingReportParser.parseVoyageNumber(voyageNumberStr, errors);
            var eventType = HandlingReportParser.parseEventType(eventTypeStr, errors);
            var unLocode = HandlingReportParser.parseUnLocode(unLocodeStr, errors);
            var operatorCode = HandlingReportParser.parseOperatorCode();
            
            if(!errors.Any())
            {
                handlingEventService.registerHandlingEvent(date, trackingId, voyageNumber, unLocode, eventType, operatorCode);
            }
            else
            {
                throw new FormatException(String.Join(", ", errors));
            }
        }

        private void delete(FileInfo file)
        {
            try
            {
                file.Delete();
            }

            catch(Exception)
            {
                logger.Error("Could not delete " + file.Name);
                throw;
            }
        }

        private void move(FileInfo file)
        {
            var destination = Path.Combine(parseFailureDirectory.FullName, file.Name);
            try
            {
                file.MoveTo(destination);
            }

            catch(Exception)
            {
                logger.Error("Could not move " + file.Name + " to " + destination);
                throw;
            }
        }

        public void AfterPropertiesSet()
        {
            if(uploadDirectory.FullName.Equals(parseFailureDirectory.FullName))
            {
                throw new ArgumentException("Upload and parse failed directories must not be the same directory: " + uploadDirectory);
            }
            
            if(!uploadDirectory.Exists)
            {
                uploadDirectory.Create();
            }
            
            if(!parseFailureDirectory.Exists)
            {
                parseFailureDirectory.Create();
            }
        }

        public void setUploadDirectory(DirectoryInfo uploadDirectory)
        {
            this.uploadDirectory = uploadDirectory;
        }

        public void setParseFailureDirectory(DirectoryInfo parseFailureDirectory)
        {
            this.parseFailureDirectory = parseFailureDirectory;
        }

        public void setHandlingEventService(HandlingEventService handlingEventService)
        {
            this.handlingEventService = handlingEventService;
        }
    }
}