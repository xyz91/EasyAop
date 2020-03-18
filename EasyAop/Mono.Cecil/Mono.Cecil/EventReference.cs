namespace Mono.Cecil
{
	public abstract class EventReference : MemberReference
	{
		private TypeReference event_type;

		public TypeReference EventType
		{
			get
			{
				return event_type;
			}
			set
			{
				event_type = value;
			}
		}

		public override string FullName => event_type.FullName + " " + base.MemberFullName();

		protected EventReference(string name, TypeReference eventType)
			: base(name)
		{
			Mixin.CheckType(eventType, Mixin.Argument.eventType);
			event_type = eventType;
		}

		protected override IMemberDefinition ResolveDefinition()
		{
			return Resolve();
		}

		public new abstract EventDefinition Resolve();
	}
}
