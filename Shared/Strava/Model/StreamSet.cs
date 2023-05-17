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
    /// StreamSet
    /// </summary>
    [DataContract]
        public partial class StreamSet :  IEquatable<StreamSet>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StreamSet" /> class.
        /// </summary>
        /// <param name="time">time.</param>
        /// <param name="distance">distance.</param>
        /// <param name="latlng">latlng.</param>
        /// <param name="altitude">altitude.</param>
        /// <param name="velocitySmooth">velocitySmooth.</param>
        /// <param name="heartrate">heartrate.</param>
        /// <param name="cadence">cadence.</param>
        /// <param name="watts">watts.</param>
        /// <param name="temp">temp.</param>
        /// <param name="moving">moving.</param>
        /// <param name="gradeSmooth">gradeSmooth.</param>
        public StreamSet(TimeStream time = default(TimeStream), DistanceStream distance = default(DistanceStream), LatLngStream latlng = default(LatLngStream), AltitudeStream altitude = default(AltitudeStream), SmoothVelocityStream velocitySmooth = default(SmoothVelocityStream), HeartrateStream heartrate = default(HeartrateStream), CadenceStream cadence = default(CadenceStream), PowerStream watts = default(PowerStream), TemperatureStream temp = default(TemperatureStream), MovingStream moving = default(MovingStream), SmoothGradeStream gradeSmooth = default(SmoothGradeStream))
        {
            this.Time = time;
            this.Distance = distance;
            this.Latlng = latlng;
            this.Altitude = altitude;
            this.VelocitySmooth = velocitySmooth;
            this.Heartrate = heartrate;
            this.Cadence = cadence;
            this.Watts = watts;
            this.Temp = temp;
            this.Moving = moving;
            this.GradeSmooth = gradeSmooth;
        }
        
        /// <summary>
        /// Gets or Sets Time
        /// </summary>
        [DataMember(Name="time", EmitDefaultValue=false)]
        public TimeStream Time { get; set; }

        /// <summary>
        /// Gets or Sets Distance
        /// </summary>
        [DataMember(Name="distance", EmitDefaultValue=false)]
        public DistanceStream Distance { get; set; }

        /// <summary>
        /// Gets or Sets Latlng
        /// </summary>
        [DataMember(Name="latlng", EmitDefaultValue=false)]
        public LatLngStream Latlng { get; set; }

        /// <summary>
        /// Gets or Sets Altitude
        /// </summary>
        [DataMember(Name="altitude", EmitDefaultValue=false)]
        public AltitudeStream Altitude { get; set; }

        /// <summary>
        /// Gets or Sets VelocitySmooth
        /// </summary>
        [DataMember(Name="velocity_smooth", EmitDefaultValue=false)]
        public SmoothVelocityStream VelocitySmooth { get; set; }

        /// <summary>
        /// Gets or Sets Heartrate
        /// </summary>
        [DataMember(Name="heartrate", EmitDefaultValue=false)]
        public HeartrateStream Heartrate { get; set; }

        /// <summary>
        /// Gets or Sets Cadence
        /// </summary>
        [DataMember(Name="cadence", EmitDefaultValue=false)]
        public CadenceStream Cadence { get; set; }

        /// <summary>
        /// Gets or Sets Watts
        /// </summary>
        [DataMember(Name="watts", EmitDefaultValue=false)]
        public PowerStream Watts { get; set; }

        /// <summary>
        /// Gets or Sets Temp
        /// </summary>
        [DataMember(Name="temp", EmitDefaultValue=false)]
        public TemperatureStream Temp { get; set; }

        /// <summary>
        /// Gets or Sets Moving
        /// </summary>
        [DataMember(Name="moving", EmitDefaultValue=false)]
        public MovingStream Moving { get; set; }

        /// <summary>
        /// Gets or Sets GradeSmooth
        /// </summary>
        [DataMember(Name="grade_smooth", EmitDefaultValue=false)]
        public SmoothGradeStream GradeSmooth { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class StreamSet {\n");
            sb.Append("  Time: ").Append(Time).Append("\n");
            sb.Append("  Distance: ").Append(Distance).Append("\n");
            sb.Append("  Latlng: ").Append(Latlng).Append("\n");
            sb.Append("  Altitude: ").Append(Altitude).Append("\n");
            sb.Append("  VelocitySmooth: ").Append(VelocitySmooth).Append("\n");
            sb.Append("  Heartrate: ").Append(Heartrate).Append("\n");
            sb.Append("  Cadence: ").Append(Cadence).Append("\n");
            sb.Append("  Watts: ").Append(Watts).Append("\n");
            sb.Append("  Temp: ").Append(Temp).Append("\n");
            sb.Append("  Moving: ").Append(Moving).Append("\n");
            sb.Append("  GradeSmooth: ").Append(GradeSmooth).Append("\n");
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
            return this.Equals(input as StreamSet);
        }

        /// <summary>
        /// Returns true if StreamSet instances are equal
        /// </summary>
        /// <param name="input">Instance of StreamSet to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(StreamSet input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.Time == input.Time ||
                    (this.Time != null &&
                    this.Time.Equals(input.Time))
                ) && 
                (
                    this.Distance == input.Distance ||
                    (this.Distance != null &&
                    this.Distance.Equals(input.Distance))
                ) && 
                (
                    this.Latlng == input.Latlng ||
                    (this.Latlng != null &&
                    this.Latlng.Equals(input.Latlng))
                ) && 
                (
                    this.Altitude == input.Altitude ||
                    (this.Altitude != null &&
                    this.Altitude.Equals(input.Altitude))
                ) && 
                (
                    this.VelocitySmooth == input.VelocitySmooth ||
                    (this.VelocitySmooth != null &&
                    this.VelocitySmooth.Equals(input.VelocitySmooth))
                ) && 
                (
                    this.Heartrate == input.Heartrate ||
                    (this.Heartrate != null &&
                    this.Heartrate.Equals(input.Heartrate))
                ) && 
                (
                    this.Cadence == input.Cadence ||
                    (this.Cadence != null &&
                    this.Cadence.Equals(input.Cadence))
                ) && 
                (
                    this.Watts == input.Watts ||
                    (this.Watts != null &&
                    this.Watts.Equals(input.Watts))
                ) && 
                (
                    this.Temp == input.Temp ||
                    (this.Temp != null &&
                    this.Temp.Equals(input.Temp))
                ) && 
                (
                    this.Moving == input.Moving ||
                    (this.Moving != null &&
                    this.Moving.Equals(input.Moving))
                ) && 
                (
                    this.GradeSmooth == input.GradeSmooth ||
                    (this.GradeSmooth != null &&
                    this.GradeSmooth.Equals(input.GradeSmooth))
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
                if (this.Time != null)
                    hashCode = hashCode * 59 + this.Time.GetHashCode();
                if (this.Distance != null)
                    hashCode = hashCode * 59 + this.Distance.GetHashCode();
                if (this.Latlng != null)
                    hashCode = hashCode * 59 + this.Latlng.GetHashCode();
                if (this.Altitude != null)
                    hashCode = hashCode * 59 + this.Altitude.GetHashCode();
                if (this.VelocitySmooth != null)
                    hashCode = hashCode * 59 + this.VelocitySmooth.GetHashCode();
                if (this.Heartrate != null)
                    hashCode = hashCode * 59 + this.Heartrate.GetHashCode();
                if (this.Cadence != null)
                    hashCode = hashCode * 59 + this.Cadence.GetHashCode();
                if (this.Watts != null)
                    hashCode = hashCode * 59 + this.Watts.GetHashCode();
                if (this.Temp != null)
                    hashCode = hashCode * 59 + this.Temp.GetHashCode();
                if (this.Moving != null)
                    hashCode = hashCode * 59 + this.Moving.GetHashCode();
                if (this.GradeSmooth != null)
                    hashCode = hashCode * 59 + this.GradeSmooth.GetHashCode();
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
