using System.Text;

namespace MtgInventory.Service.Models
{
    public class QueryCardOptions
    {
        public string Name { get; set; }
        public bool IsBasicLand { get; set; }
        public bool IsToken { get; set; }

        public override string ToString()
        {
            var result = new StringBuilder();

            result.Append($"'{Name}'");

            if (IsBasicLand)
            {
                result.Append(", BasicLand=true");
            }
            
            if (IsToken)
            {
                result.Append(", Token=true");
            }

            return result.ToString();
        }
    }
}