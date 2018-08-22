using System;
using System.Collections;
using System.Management.Automation;
using Google.Cloud.Datastore.V1;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Google.Type;
using Value = Google.Cloud.Datastore.V1.Value;

namespace Google.PowerShell.Datastore
{
    /// <summary>
    /// Creates a new entity object ready for insertion/updating
    /// </summary>
    [Cmdlet(VerbsCommon.New, "GdsEntity")]
    [OutputType(typeof(Entity))]
    public class NewGdsEntityCmdlet : GdsCmdlet
    {
        [Parameter(Mandatory = false, Position = 0)]
        [ValidateNotNullOrEmpty()]
        public string Key { get; set; }

        [Parameter(Mandatory = true, Position = 1)]
        public Hashtable Properties { get; set; }

        [Parameter(Mandatory = false, Position = 2)]
        public override string Project { get; set; }

        [Parameter(Mandatory = false, Position = 3)]
        [ValidateNotNullOrEmpty()]
        public string Kind { get; set; }

        private Key _gKey;

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            var db = DatastoreDb.Create(Project);
            var keyFactory = db.CreateKeyFactory(Kind);

            _gKey = string.IsNullOrEmpty(Key) ? keyFactory.CreateIncompleteKey() : keyFactory.CreateKey(Key);
            
            var entity = new Entity();
            entity.Key = _gKey;
            foreach (var k in Properties.Keys)
            {
                var val = new Value();
                
                switch (Properties[k])
                {// I'm surprised there's no built in method in Value to handle this scenario
                    case NullValue c:
                    {
                        val.NullValue = c;
                        break;
                    }
                    case bool c:
                    {
                        val.BooleanValue = c;
                        break;
                    }
                    case int c:
                    {
                        val.IntegerValue = (long) c;
                        break;
                    }
                    case long c:
                    {
                        val.IntegerValue = c;
                        break;
                    }
                    case float c:
                    {
                        val.DoubleValue = (double) c;
                        break;
                    }
                    case double c:
                    {
                        val.DoubleValue = c;
                        break;
                    }
                    case Timestamp c:
                    {
                        val.TimestampValue = c;
                        break;
                    }
                    case DateTime c:
                    {
                        val.TimestampValue = c.ToTimestamp();
                        break;
                    }
                    case Cloud.Datastore.V1.Key c:
                    {
                        val.KeyValue = c;
                        break;
                    }
                    case string c:
                    {
                        val.StringValue = c;
                        break;
                    }
                    case byte[] c:
                    {
                        if (c.Length > 1000000)
                        {
                            var e = new ArgumentException("Max byte count exceeded");
                            var er = new ErrorRecord(e, "ByteCountLimitExceeded", ErrorCategory.InvalidData, Properties);
                            ThrowTerminatingError(er);
                        }

                        val.BlobValue = ByteString.CopyFrom(c);
                        break;
                    }
                    case LatLng c:
                    {
                        val.GeoPointValue = c;
                        break;
                    }
                    case Entity c:
                    {
                        val.EntityValue = c;
                        break;
                    }
                    case ArrayValue c:
                    {
                        val.ArrayValue = c;
                        break;
                    }
                    default:
                    {
                        // It's either this or throw - not sure of best approach
                        WriteWarning($"Unexpected type in properties dictionary: {Properties[k].GetType().FullName}");
                        val.StringValue = Properties[k].ToString();
                        break;
                    }

                }
                
                entity.Properties.Add(k.ToString(), val);
            }

            WriteObject(entity);
        }
    }
}