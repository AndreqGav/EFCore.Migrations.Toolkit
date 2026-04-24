namespace EFCore.Migrations.Triggers.PostgreSQL.Enums
{
    public enum ConstraintTriggerType
    {
        /// <summary>
        /// CONSTRAINT TRIGGER ... NOT DEFERRABLE
        /// </summary>
        NotDeferrable = 1,

        /// <summary>
        /// CONSTRAINT TRIGGER ... DEFERRABLE INITIALLY IMMEDIATE
        /// </summary>
        DeferrableInitiallyImmediate = 2,

        /// <summary>
        /// CONSTRAINT TRIGGER ... DEFERRABLE INITIALLY DEFERRED
        /// </summary>
        DeferrableInitiallyDeferred = 3,
    }
}