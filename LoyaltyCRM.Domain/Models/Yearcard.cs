using System.ComponentModel.DataAnnotations.Schema;
using LoyaltyCRM.Domain.DomainPrimitives;
using Microsoft.Extensions.DependencyInjection;

namespace LoyaltyCRM.Domain.Models
{
    public class Yearcard : WithTimestamps
    {
        public Guid? Id { get; }
        public Name? Name { get; set; }
        public CardNumber? CardId { get; set; }

        [NotMapped]
        public bool IsValidForDiscount { get; set; } = false;
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

        public bool IsYearcardSetForDeletion(int deleteGracePeriodDays)
        {
            foreach (ValidityInterval interval in ValidityIntervals)
            {
                if (interval.EndDate.Value.AddDays(deleteGracePeriodDays) >= DateTime.Now)
                {
                    return true;
                }
            }
            return false;
        }

        public void SetIsYearcardValidForDiscount(int discountGracePeriodDays)
        {
            foreach (ValidityInterval interval in ValidityIntervals)
            {
                if (interval.EndDate.Value.AddDays(discountGracePeriodDays) >= DateTime.Now)
                {
                    IsValidForDiscount = true;
                    return;
                }
            }
            IsValidForDiscount = false;
        }
    }
}
