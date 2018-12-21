using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities
{
    public class ShowPerson
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long ShowId { get; set; }

        public Show Show { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long PersonId { get; set; }

        public Person Person { get; set; }
    }
}