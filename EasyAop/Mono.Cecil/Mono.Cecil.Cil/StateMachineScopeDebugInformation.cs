using Mono.Collections.Generic;
using System;

namespace Mono.Cecil.Cil
{
	public sealed class StateMachineScopeDebugInformation : CustomDebugInformation
	{
		internal Collection<StateMachineScope> scopes;

		public static Guid KindIdentifier = new Guid("{6DA9A61E-F8C7-4874-BE62-68BC5630DF71}");

		public Collection<StateMachineScope> Scopes => scopes ?? (scopes = new Collection<StateMachineScope>());

		public override CustomDebugInformationKind Kind => CustomDebugInformationKind.StateMachineScope;

		public StateMachineScopeDebugInformation()
			: base(KindIdentifier)
		{
		}
	}
}
