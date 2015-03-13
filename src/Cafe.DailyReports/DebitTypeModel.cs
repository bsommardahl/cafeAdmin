namespace Cafe.DailyReports
{
    public class DebitTypeModel
    {
        public DebitTypeModel(string type)
        {
            Name = type;
        }

        public string Name { get; private set; }
    }
}