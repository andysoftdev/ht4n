// This file is used by Code Analysis to maintain 
// CA_GLOBAL_SUPPRESS_MESSAGE macros that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Error List, point to "Suppress Message(s)", and click 
// "In Project Suppression File".
// You do not need to add suppressions to this file manually.

CA_GLOBAL_SUPPRESS_MESSAGE("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="1", Scope="member", Target="Hypertable.ChunkedTableMutator.#Set(Hypertable.Key,System.Byte[],System.Boolean)");
CA_GLOBAL_SUPPRESS_MESSAGE("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId="Hypertable.Logging.TraceEvent(System.Diagnostics.TraceEventType,System.String)", Scope="member", Target="Hypertable.Context.#.cctor()");
CA_GLOBAL_SUPPRESS_MESSAGE("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Scope="member", Target="Hypertable.Context.#RegisterProvider(System.String,System.Func`2<System.Collections.Generic.IDictionary`2<System.String,System.Object>,Hypertable.IContext>)");
CA_GLOBAL_SUPPRESS_MESSAGE("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Scope="member", Target="Hypertable.HypertableException.#Create({modopt(System.Runtime.CompilerServices.IsConst)}ht4c.Common.HypertableException{modopt(System.Runtime.CompilerServices.IsImplicitlyDereferenced)}*)");
CA_GLOBAL_SUPPRESS_MESSAGE("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Scope="member", Target="Hypertable.HypertableException.#GetObjectData(System.Runtime.Serialization.SerializationInfo,System.Runtime.Serialization.StreamingContext)");
CA_GLOBAL_SUPPRESS_MESSAGE("Microsoft.Security", "CA2102:CatchNonClsCompliantExceptionsInGeneralHandlers", Scope="member", Target="Hypertable.QueuedTableMutator.#SetCell()");
CA_GLOBAL_SUPPRESS_MESSAGE("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames");
CA_GLOBAL_SUPPRESS_MESSAGE("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Scope="member", Target="Hypertable.Composition.IContextProvider.#Provider");
CA_GLOBAL_SUPPRESS_MESSAGE("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId="ProviderName", Scope="member", Target="Hypertable.Context.#.cctor()");
CA_GLOBAL_SUPPRESS_MESSAGE("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="1", Scope="member", Target="Hypertable.ScanSpec.#DistictColumn(System.Collections.Generic.IEnumerable`1<System.String>,System.Collections.Generic.ISet`1<System.String>&)");
