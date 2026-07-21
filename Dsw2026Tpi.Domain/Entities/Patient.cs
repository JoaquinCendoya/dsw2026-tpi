using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public class Patient : EntityBase
    {
        public string Dni { get; init; }
        public string FullName { get; private set; }
        public string? Phone { get; private set; }
        public bool IsActive { get; private set; }

        #region Constructor for EF
#pragma warning disable CS8618
        private Patient()
        {
        }
#pragma warning restore CS8618
        #endregion

        public Patient(string dni, string fullName, string? phone = null, Guid? id = null) : base(id)
        {
            Dni = dni;
            FullName = fullName;
            Phone = phone;
            IsActive = true;
        }

        public void UpdateContactInfo(string fullName, string? phone)
        {
            FullName = fullName;
            Phone = phone;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}
