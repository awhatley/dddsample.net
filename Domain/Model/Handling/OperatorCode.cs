using DomainDrivenDelivery.Domain.Patterns;
using DomainDrivenDelivery.Domain.Patterns.ValueObject;
using DomainDrivenDelivery.Utilities;

namespace DomainDrivenDelivery.Domain.Model.Handling
{
    /// <summary>
    /// Port operators are assigned an operator code.
    /// </summary>
    public class OperatorCode : ValueObjectSupport<OperatorCode>
    {
        private readonly string code;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="code">code, five letters (ex: "ABCDE")</param>
        public OperatorCode(string code)
        {
            Validate.notEmpty(code, "Code is required");
            Validate.isTrue(code.Length == 5, "Operator codes must be exactly five letters: " + code);
            this.code = code;
        }

        /// <summary>
        /// The operator code as a String
        /// </summary>
        /// <returns>The operator code as a String</returns>
        public string stringValue()
        {
            return code;
        }

        public override string ToString()
        {
            return stringValue();
        }

        OperatorCode()
        {
            // Needed by Hibernate
            code = null;
        }
    }
}