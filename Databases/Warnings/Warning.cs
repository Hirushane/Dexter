﻿using System.ComponentModel.DataAnnotations;

namespace Dexter.Databases.Warnings {

    /// <summary>
    /// The Warning class contains information on a warning, such as the ID, issuer, user and reason.
    /// It is stored in the WarningsDB and can be pulled from its related Warnings DBSet.
    /// </summary>
    public class Warning {

        /// <summary>
        /// The WarningID is the KEY of the table.
        /// Every warning has a unique ID, and this increases on the amount of warnings filled.
        /// </summary>
        [Key]
        public int WarningID { get; set; }

        /// <summary>
        /// The Issuer field is the snowflake ID of the moderator that has warned the user.
        /// </summary>
        public ulong Issuer { get; set; }

        /// <summary>
        /// The User field is the snowflake ID of the user that has been warned.
        /// </summary>
        public ulong User { get; set; }

        /// <summary>
        /// The Reason field is a description of what the warning was given for.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// The EntryType field specifies if the warning is still valid or if it has been revoked.
        /// </summary>
        public EntryType EntryType { get; set; }

        /// <summary>
        /// The Time Of Issue field is a long field of the UNIX time at which the warning had been issued on.
        /// </summary>
        public long TimeOfIssue { get; set; }

    }

}
