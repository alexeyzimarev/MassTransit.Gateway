using System;
using System.Reflection;
using System.Reflection.Emit;

namespace MassTransit.Gateway.Dynamics
{
    public class MessageTypeProvider
    {
        private readonly ModuleBuilder _moduleBuilder;

        public MessageTypeProvider()
        {
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName(Guid.NewGuid().ToString()),
                AssemblyBuilderAccess.Run);

            _moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");
        }

        public Type BuildMessageType(MessageTypeDefinition messageTypeDefinition)
        {
            var typeBuilder = _moduleBuilder.DefineType(messageTypeDefinition.ClassName, TypeAttributes.Public);

            foreach (var propertyDefinition in messageTypeDefinition.PropertyDefinitions)
            {
                AddProperty(typeBuilder, propertyDefinition.Name, propertyDefinition.Type);
            }

            return typeBuilder.CreateTypeInfo();
        }

        private static void AddProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
        {
            var fieldBuilder = typeBuilder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

            var propertyBuilder = typeBuilder.DefineProperty(propertyName,
                PropertyAttributes.HasDefault, propertyType, null);

            const MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName |
                                                MethodAttributes.HideBySig;

            var getterBuilder=
                typeBuilder.DefineMethod("get_" + propertyName,
                    getSetAttr,
                    propertyType,
                    Type.EmptyTypes);

            var getterIlGenerator = getterBuilder.GetILGenerator();

            getterIlGenerator.Emit(OpCodes.Ldarg_0);
            getterIlGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
            getterIlGenerator.Emit(OpCodes.Ret);

            var setterBuilder =
                typeBuilder.DefineMethod("set_" + propertyName,
                    getSetAttr,
                    null,
                    new[] {propertyType});

            var setterIlGenerator = setterBuilder.GetILGenerator();

            setterIlGenerator.Emit(OpCodes.Ldarg_0);
            setterIlGenerator.Emit(OpCodes.Ldarg_1);
            setterIlGenerator.Emit(OpCodes.Stfld, fieldBuilder);
            setterIlGenerator.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getterBuilder);
            propertyBuilder.SetSetMethod(setterBuilder);
        }
    }
}