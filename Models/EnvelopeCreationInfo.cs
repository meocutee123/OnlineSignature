using DocuSign.eSign.Model;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnlineSignature.Models
{
    public class EnvelopeCreationInfo : DsAccessInfo
    {
        public EnvelopeDefinition EnvelopeDefinition { get; set; }
        
    }
}
