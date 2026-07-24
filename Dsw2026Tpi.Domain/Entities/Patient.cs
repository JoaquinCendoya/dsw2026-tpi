using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public class Patient : EntityBase
    {
        public Guid UserId { get; init; }
        public string Dni { get; init; }
        public string? FullName { get; private set; }
        public string? Phone { get; private set; }

        #region Constructor for EF
#pragma warning disable CS8618
        private Patient() { }
#pragma warning restore CS8618
        #endregion

        public Patient(Guid userId, string dni, string? fullName = null, string? phone = null, Guid? id = null) : base(id)
        {
            if (string.IsNullOrWhiteSpace(dni) || dni.Length < 7 || dni.Length > 10)
                throw new ArgumentException("DNI must be between 7 and 10 characters.");

            UserId = userId == Guid.Empty ? throw new ArgumentException("UserId is required") : userId;
            Dni = dni;
            FullName = fullName;
            Phone = phone;
        }

        public void UpdateContactInfo(string fullName, string? phone)
        {
            FullName = fullName;
            Phone = phone;
        }
    }
}
