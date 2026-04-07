using PapasCRM_API.DomainPrimitives;
using PapasCRM_API.Requests;
using PapasCRM_API.Entities;

namespace PapasCRM_API.Models
{
    public class Yearcard
    {
        public Guid? Id { get; }
        public Name? Name { get; }
        public CardNumber CardId { get; }
        // public CardValidTo ValidTo { get; }
        private int discountValidityMonths = 3;

        public List<ValidityInterval> ValidityIntervals { get; } = new List<ValidityInterval>();

        public Yearcard(Guid? id, CardNumber cardId)
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

        public bool IsYearcardValidForDiscount()
        {
            foreach (ValidityInterval interval in ValidityIntervals)
            {
                if (interval.EndDate.GetValue().AddMonths(discountValidityMonths) >= DateTime.Now)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsYearcardValid(){
            foreach (ValidityInterval interval in ValidityIntervals)
            {
                if (interval.EndDate.GetValue() <= DateTime.Now)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
