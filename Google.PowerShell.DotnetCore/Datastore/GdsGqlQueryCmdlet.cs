using System;
using System.Collections.Generic;
using System.Management.Automation;
using Google.Cloud.Datastore.V1;

namespace Google.PowerShell.Datastore
{
    [Cmdlet(VerbsLifecycle.Invoke, "GdsGqlQuery")]
    [OutputType(typeof(IEnumerable<Entity>))]
    public class InvokeGdsGqlQueryCmdlet : GdsCmdlet
    {
        [Parameter(Mandatory = false, Position = 0)]
        public override string Project { get; set; }

        [Parameter(Mandatory = true, Position = 1)]
        public string Query { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter AllowLiterals { get; set; }


        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            var db = DatastoreDb.Create(Project);
            var query = new GqlQuery
            {
                AllowLiterals = AllowLiterals.ToBool(),
                QueryString = Query
            };

            var results = db.RunQuery(query);

            foreach (var ent in results.Entities)
            {
                WriteObject(ent);
            }
        }
    }

    /// <summary>
    /// Covers insert, update and upsert actions
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "GdsEntity")]
    [OutputType(typeof(void))]
    public class SetGdsEntityCmdlet : GdsCmdlet
    {

        [Parameter(Mandatory = false, Position = 0)]
        public override string Project { get; set; }

        /// <summary>
        /// <Description>The entity to be acted upon. This entity can be created using the New-GdsEntity cmdlet. This parameter can be received from the pipeline.</Description>
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ValueFromPipeline = true)]
        public Entity[] Entity { get; set; }

        [Parameter(Mandatory = false, Position = 2)]
        [ValidateSet("Insert", "Update", "Upsert")]
        public string Action { get; set; } = "Upsert";

        private IReadOnlyList<Key> returnKey;

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            var db = DatastoreDb.Create(Project);
            using (var transaction = db.BeginTransaction())
            {
                switch (Action)
                {
                    case "Insert":
                        returnKey = db.Insert(Entity);
                        break;
                    case "Update":
                        db.Update(Entity);
                        break;
                    case "Upsert":
                        returnKey = db.Upsert(Entity);
                        break;
                    default:
                        var e = new ArgumentException();
                        var er = new ErrorRecord(e, "UnknownAction", ErrorCategory.InvalidArgument, Action);
                        ThrowTerminatingError(er);
                        break;
                }

                WriteObject(returnKey);
            }
            
        }


    }
}