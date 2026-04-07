using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PapasCRM_API.Requests.PropertyInterfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace PapasCRM_API.Requests.Yearcard
{
    public class YearcardGetUnconfirmedResponse : IHasId, IHasCardId, IHasIsValidForDiscount, IHasExposedIdentification
    {
        public Guid? Id { get; set; }

        [Range(1, 100000)]
        public int CardId { get; set; }

        public string ExposedIdentification { get; set; } = string.Empty;

        [NotMapped]
        public bool IsValidForDiscount { get; set; }

    }
}