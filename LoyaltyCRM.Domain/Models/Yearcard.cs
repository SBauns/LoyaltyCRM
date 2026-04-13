using LoyaltyCRM.Domain.DomainPrimitives;

namespace LoyaltyCRM.Domain.Models
{
    public class Yearcard : WithTimestamps
    {
        public Guid? Id { get; }
        public Name? Name { get; set; }
        public CardNumber? CardId { get; set; }
        // public CardValidTo ValidTo { get; }

        //Relationships
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public List<ValidityInterval> ValidityIntervals { get; set; } = new List<ValidityInterval>();

        public Yearcard()
        {
            //EF CORE
        }
        public Yearcard(Guid? id, CardNumber? cardId)
        {
            Id = id;
            CardId = cardId;
        }

        public Yearcard(Guid? id, CardNumber cardId, Name name) 
            : this(id, cardId){
            Name = name;
        }

        public void AddValidityInterval(ValidityInterval validityInterval)
        {
            if (validityInterval == null)
            {
                throw new ArgumentNullException(nameof(validityInterval), "Validity interval cannot be null");
            }

            ValidityIntervals.Add(validityInterval);
        }

        public bool IsYearcardSetForDeletion(int gracePeriodDays)
        {
            foreach (ValidityInterval interval in ValidityIntervals)
            {
                if (interval.EndDate.Value.AddDays(gracePeriodDays) >= DateTime.Now)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
