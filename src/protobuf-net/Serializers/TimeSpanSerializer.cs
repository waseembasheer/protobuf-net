#if !NO_RUNTIME
using System;

#if FEAT_IKVM
using Type = IKVM.Reflection.Type;
using IKVM.Reflection;
#else
using System.Reflection;
#endif



namespace ProtoBuf.Serializers
{
    sealed class TimeSpanSerializer : IProtoSerializer
    {
#if FEAT_IKVM
        readonly Type expectedType;
#else
        static readonly Type expectedType = typeof(TimeSpan);
#endif
        private readonly bool wellKnown;
        public TimeSpanSerializer(DataFormat dataFormat, ProtoBuf.Meta.TypeModel model)
        {
#if FEAT_IKVM
            expectedType = model.MapType(typeof(TimeSpan));
#endif
            wellKnown = dataFormat == DataFormat.WellKnown;
        }
        public Type ExpectedType { get { return expectedType; } }

        bool IProtoSerializer.RequiresOldValue { get { return false; } }
        bool IProtoSerializer.ReturnsValue { get { return true; } }
#if !FEAT_IKVM
        public object Read(object value, ProtoReader source)
        {
            if (wellKnown)
            {
                return BclHelpers.ReadDuration(source);
            }
            else
            {
                Helpers.DebugAssert(value == null); // since replaces
                return BclHelpers.ReadTimeSpan(source);
            }
        }
        public void Write(object value, ProtoWriter dest)
        {
            if (wellKnown)
            {
                BclHelpers.WriteDuration((TimeSpan)value, dest);
            }
            else
            {
                BclHelpers.WriteTimeSpan((TimeSpan)value, dest);
            }
        }
#endif
#if FEAT_COMPILER
        void IProtoSerializer.EmitWrite(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            ctx.EmitWrite(ctx.MapType(typeof(BclHelpers)),
                wellKnown ? "WriteDuration" : "WriteTimeSpan", valueFrom);
        }
        void IProtoSerializer.EmitRead(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            if (wellKnown) ctx.LoadValue(valueFrom);
            ctx.EmitBasicRead(ctx.MapType(typeof(BclHelpers)),
                wellKnown ? "ReadDuration" : "ReadTimeSpan",
                ExpectedType);
        }
#endif

    }
}
#endif