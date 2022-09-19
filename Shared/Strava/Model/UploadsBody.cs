/* 
 * Strava API v3
 *
 * The [Swagger Playground](https://developers.strava.com/playground) is the easiest way to familiarize yourself with the Strava API by submitting HTTP requests and observing the responses before you write any client code. It will show what a response will look like with different endpoints depending on the authorization scope you receive from your athletes. To use the Playground, go to https://www.strava.com/settings/api and change your “Authorization Callback Domain” to developers.strava.com. Please note, we only support Swagger 2.0. There is a known issue where you can only select one scope at a time. For more information, please check the section “client code” at https://developers.strava.com/docs.
 *
 * OpenAPI spec version: 3.0.0
 * 
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;
using SwaggerDateConverter = Strava.Client.SwaggerDateConverter;

namespace Strava.Model
{
    /// <summary>
    /// UploadsBody
    /// </summary>
    [DataContract]
        public partial class UploadsBody :  IEquatable<UploadsBody>, IValidatableObject
    {
        /// <summary>
        /// The format of the uploaded file.
        /// </summary>
        /// <value>The format of the uploaded file.</value>
        [JsonConverter(typeof(StringEnumConverter))]
                public enum DataTypeEnum
        {
            /// <summary>
            /// Enum Fit for value: fit
            /// </summary>
            [EnumMember(Value = "fit")]
            Fit = 1,
            /// <summary>
            /// Enum FitGz for value: fit.gz
            /// </summary>
            [EnumMember(Value = "fit.gz")]
            FitGz = 2,
            /// <summary>
            /// Enum Tcx for value: tcx
            /// </summary>
            [EnumMember(Value = "tcx")]
            Tcx = 3,
            /// <summary>
            /// Enum TcxGz for value: tcx.gz
            /// </summary>
            [EnumMember(Value = "tcx.gz")]
            TcxGz = 4,
            /// <summary>
            /// Enum Gpx for value: gpx
            /// </summary>
            [EnumMember(Value = "gpx")]
            Gpx = 5,
            /// <summary>
            /// Enum GpxGz for value: gpx.gz
            /// </summary>
            [EnumMember(Value = "gpx.gz")]
            GpxGz = 6        }
        /// <summary>
        /// The format of the uploaded file.
        /// </summary>
        /// <value>The format of the uploaded file.</value>
        [DataMember(Name="data_type", EmitDefaultValue=false)]
        public DataTypeEnum? DataType { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="UploadsBody" /> class.
        /// </summary>
        /// <param name="file">The uploaded file..</param>
        /// <param name="name">The desired name of the resulting activity..</param>
        /// <param name="description">The desired description of the resulting activity..</param>
        /// <param name="trainer">Whether the resulting activity should be marked as having been performed on a trainer..</param>
        /// <param name="commute">Whether the resulting activity should be tagged as a commute..</param>
        /// <param name="dataType">The format of the uploaded file..</param>
        /// <param name="externalId">The desired external identifier of the resulting activity..</param>
        public UploadsBody(byte[] file = default(byte[]), string name = default(string), string description = default(string), string trainer = default(string), string commute = default(string), DataTypeEnum? dataType = default(DataTypeEnum?), string externalId = default(string))
        {
            this.File = file;
            this.Name = name;
            this.Description = description;
            this.Trainer = trainer;
            this.Commute = commute;
            this.DataType = dataType;
            this.ExternalId = externalId;
        }
        
        /// <summary>
        /// The uploaded file.
        /// </summary>
        /// <value>The uploaded file.</value>
        [DataMember(Name="file", EmitDefaultValue=false)]
        public byte[] File { get; set; }

        /// <summary>
        /// The desired name of the resulting activity.
        /// </summary>
        /// <value>The desired name of the resulting activity.</value>
        [DataMember(Name="name", EmitDefaultValue=false)]
        public string Name { get; set; }

        /// <summary>
        /// The desired description of the resulting activity.
        /// </summary>
        /// <value>The desired description of the resulting activity.</value>
        [DataMember(Name="description", EmitDefaultValue=false)]
        public string Description { get; set; }

        /// <summary>
        /// Whether the resulting activity should be marked as having been performed on a trainer.
        /// </summary>
        /// <value>Whether the resulting activity should be marked as having been performed on a trainer.</value>
        [DataMember(Name="trainer", EmitDefaultValue=false)]
        public string Trainer { get; set; }

        /// <summary>
        /// Whether the resulting activity should be tagged as a commute.
        /// </summary>
        /// <value>Whether the resulting activity should be tagged as a commute.</value>
        [DataMember(Name="commute", EmitDefaultValue=false)]
        public string Commute { get; set; }


        /// <summary>
        /// The desired external identifier of the resulting activity.
        /// </summary>
        /// <value>The desired external identifier of the resulting activity.</value>
        [DataMember(Name="external_id", EmitDefaultValue=false)]
        public string ExternalId { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class UploadsBody {\n");
            sb.Append("  File: ").Append(File).Append("\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  Description: ").Append(Description).Append("\n");
            sb.Append("  Trainer: ").Append(Trainer).Append("\n");
            sb.Append("  Commute: ").Append(Commute).Append("\n");
            sb.Append("  DataType: ").Append(DataType).Append("\n");
            sb.Append("  ExternalId: ").Append(ExternalId).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
  
        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="input">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object input)
        {
            return this.Equals(input as UploadsBody);
        }

        /// <summary>
        /// Returns true if UploadsBody instances are equal
        /// </summary>
        /// <param name="input">Instance of UploadsBody to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(UploadsBody input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.File == input.File ||
                    (this.File != null &&
                    this.File.Equals(input.File))
                ) && 
                (
                    this.Name == input.Name ||
                    (this.Name != null &&
                    this.Name.Equals(input.Name))
                ) && 
                (
                    this.Description == input.Description ||
                    (this.Description != null &&
                    this.Description.Equals(input.Description))
                ) && 
                (
                    this.Trainer == input.Trainer ||
                    (this.Trainer != null &&
                    this.Trainer.Equals(input.Trainer))
                ) && 
                (
                    this.Commute == input.Commute ||
                    (this.Commute != null &&
                    this.Commute.Equals(input.Commute))
                ) && 
                (
                    this.DataType == input.DataType ||
                    (this.DataType != null &&
                    this.DataType.Equals(input.DataType))
                ) && 
                (
                    this.ExternalId == input.ExternalId ||
                    (this.ExternalId != null &&
                    this.ExternalId.Equals(input.ExternalId))
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hashCode = 41;
                if (this.File != null)
                    hashCode = hashCode * 59 + this.File.GetHashCode();
                if (this.Name != null)
                    hashCode = hashCode * 59 + this.Name.GetHashCode();
                if (this.Description != null)
                    hashCode = hashCode * 59 + this.Description.GetHashCode();
                if (this.Trainer != null)
                    hashCode = hashCode * 59 + this.Trainer.GetHashCode();
                if (this.Commute != null)
                    hashCode = hashCode * 59 + this.Commute.GetHashCode();
                if (this.DataType != null)
                    hashCode = hashCode * 59 + this.DataType.GetHashCode();
                if (this.ExternalId != null)
                    hashCode = hashCode * 59 + this.ExternalId.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// To validate all properties of the instance
        /// </summary>
        /// <param name="validationContext">Validation context</param>
        /// <returns>Validation Result</returns>
        IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            yield break;
        }
    }
}