using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnlineSignature.Models
{
    public class EnvelopeCreationRequest
    {
        [Required]
        public string SignerName { get; set; }
        [Required]
        public string SignerEmail { get; set; }
        [Required]
        public string SignerClientId { get; set; }
        [Required]
        public List<IFormFile> DocPDFs { get; set; }
        //[Required]
        //public string DsReturnUrl { get; set; }
        //[Required]
        //public string DsPingUrl { get; set; }
    }
}
