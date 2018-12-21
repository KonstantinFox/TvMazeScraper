using System.Collections.Generic;

namespace BusinessLogic.Model
{
    public class Show
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public List<CastMember> Cast { get; set; }
    }
}