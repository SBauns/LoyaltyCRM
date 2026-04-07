using Azure.Identity;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PapasCRM_API.DomainPrimitives;
using PapasCRM_API.Requests;
using PapasCRM_API.Entities;
using PapasCRM_API.Enums;
using System.ComponentModel.DataAnnotations;

namespace PapasCRM_API.Models
{
    public class ApplicationUser
    {
        public string? Id { get; }
        public PhoneNumber PhoneNumber { get; }
        public Email Email { get; }
        public UserName UserName { get; }

        public ApplicationUser(string? id, PhoneNumber phoneNumber, Email email, UserName userName)
        {
            Id = id;
            PhoneNumber = phoneNumber;
            Email = email;
            UserName = userName;
        }
    }
}
