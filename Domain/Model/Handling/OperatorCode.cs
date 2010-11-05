using DomainDrivenDelivery.Domain.Patterns.ValueObject;
using DomainDrivenDelivery.Utilities;

namespace DomainDrivenDelivery.Domain.Model.Handling
{
    /// <summary>
    /// Port operators are assigned an operator code.
    /// </summary>
    public class OperatorCode : ValueObjectSupport<OperatorCode>
    {
        /// <summary>
        /// The operator code as a String
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="code">code, five letters (ex: "ABCDE")</param>
        public OperatorCode(string code)
        {
            Validate.notEmpty(code, "Code is required");
            Validate.isTrue(code.Length == 5, "Operator codes must be exactly five letters: " + code);
            
            Value = code;
        }

        public override string ToString()
        {
            return Value;
        }

        protected internal OperatorCode()
        {
        }
    }
}