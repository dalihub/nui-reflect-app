/*
 * Copyright (c) 2018 Samsung Electronics Co., Ltd.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Linq;
using Tizen.NUI;
using Tizen.NUI.UIComponents;
using Tizen.NUI.BaseComponents;
using Tizen.NUI.Constants;

// See https://docs.microsoft.com/en-us/dotnet/api/system.reflection.memberinfo?view=netframework-4.7

namespace GetSyms
{
    class TypeHelper
    {
        Dictionary<string, string> _operators;

        TypeHelper()
        {
            _operators = Operators();
        }

        static private Dictionary<string, string> Operators()
        {
            Dictionary<string, string>  ops = new Dictionary<string, string>();
            ops.Add("op_Implicit", "operator implicit");
            ops.Add("op_Explicit", "operator explicit");
            ops.Add("op_Addition", "operator+");
            ops.Add("op_Subtraction", "operator-");
            ops.Add("op_Multiply","operator*");
            ops.Add("op_Division","operator/");
            ops.Add("op_Modulus","operator%");
            ops.Add("op_ExclusiveOr","operator^");
            ops.Add("op_BitwiseAnd","operator&");
            ops.Add("op_BitwiseOr","operator|");
            ops.Add("op_LogicalAnd","operator&&");
            ops.Add("op_LogicalOr","operator||");
            ops.Add("op_Assign","operator=");
            ops.Add("op_LeftShift","operator<<");
            ops.Add("op_RightShift","operator>>");
            ops.Add("op_SignedRightShift","operator>>");
            ops.Add("op_UnsignedRightShift","operator>>");
            ops.Add("op_Equality","operator==");
            ops.Add("op_GreaterThan","operator>");
            ops.Add("op_LessThan","operator<");
            ops.Add("op_Inequality","operator!=");
            ops.Add("op_GreaterThanOrEqual","operator>=");
            ops.Add("op_LessThanOrEqual","operator<=");
            ops.Add("op_MultiplicationAssignment","operator*=");
            ops.Add("op_SubtractionAssignment","operator-=");
            ops.Add("op_ExclusiveOrAssignment","operator^=");
            ops.Add("op_LeftShiftAssignment","operator<<=");
            ops.Add("op_ModulusAssignment","operator%=");
            ops.Add("op_AdditionAssignment","operator+=");
            ops.Add("op_BitwiseAndAssignment","operator&=");
            ops.Add("op_BitwiseOrAssignment","operator|=");
            ops.Add("op_Comma","operator,");
            ops.Add("op_DivisionAssignment","operator/=");
            ops.Add("op_Decrement","operator--");
            ops.Add("op_Increment","operator++");
            ops.Add("op_UnaryNegation","operator-");
            ops.Add("op_UnaryPlus","operator+");
            ops.Add("op_OnesComplement","operator~");
            return ops;
        }

        void WriteTypeName( System.Type type )
        {
            if( type.DeclaringType != null )
            {
                Console.Write( type.DeclaringType.Name + "." + type.Name + "," + type.FullName );
            }
            else
            {
                Console.Write( type.Name + "," + type.FullName );
            }
        }

        public void WriteMethod( System.Type type, bool isStatic, System.Reflection.MethodInfo method )
        {
            WriteTypeName(type);
            Console.WriteLine("," + method.Name +
                              ",Public Method" +
                              "," + // property scope column
                              ",\"" + (isStatic?"static ":"") + method.ToString() + "\"");
        }

        public void WriteOperator( System.Type type, System.Reflection.MethodInfo method )
        {
            string value;
            bool found = _operators.TryGetValue(method.Name, out value);

            WriteTypeName(type);
            Console.WriteLine("," + (found?value:method.Name) +
                              ",Operator" +
                              "," + // property scope column
                              ",\"" + method.ToString() + "\"");
        }

        public void WriteProperty( System.Type type, PropertyInfo property, bool setMethod )
        {
            WriteTypeName(type);
            Console.WriteLine("," + property.Name +
                              ",Property," +
                              (setMethod?"r/w":"r/o") + "," + property.PropertyType.Name);
        }

        public void WriteEvent( System.Type type, string ev )
        {
            WriteTypeName(type);
            Console.WriteLine("," + ev +
                              ",Event");
        }

        public void WriteEnum( System.Type type, System.Type subType )
        {
            WriteTypeName(type);
            if( subType != null )
            {
                Console.Write("," + subType.Name +
                              ",Enum,,");
                WriteEnumDetail( subType );
            }
            else
            {
                Console.Write(",,Enum,," );

                WriteEnumDetail( type );
            }
            Console.WriteLine();
        }

        public void WriteEnumDetail( System.Type type )
        {
            var enumName=type.Name;
            Console.Write("\"{");
            bool first=true;
            foreach( var fieldInfo in type.GetFields() )
            {
                if( fieldInfo.FieldType.GetTypeInfo().IsEnum )
                {
                    var fName=fieldInfo.Name;
                    var fValue=fieldInfo.GetRawConstantValue();
                    if(!first)
                        Console.Write(", ");
                    first = false;
                    Console.Write(fName + "=" + fValue);
                }
            }
            Console.Write("}\"");
        }

        public void WriteType( System.Type type )
        {
            System.Reflection.TypeInfo typeInfo = type.GetTypeInfo();
            if( typeInfo.BaseType != typeof(System.MulticastDelegate) &&
                typeInfo.BaseType != typeof(System.EventArgs) &&
                typeInfo.Namespace.StartsWith( "Tizen.NUI" ) )
            {
                if( typeInfo.IsEnum )
                {
                    WriteEnum( type, null );
                    return;
                }
                if( type.Name == "Vector2" )
                {
                    var x=1;
                }
                var staticMethodInfos = type.GetMethods(BindingFlags.Public|BindingFlags.Static|BindingFlags.DeclaredOnly);
                var instanceMethodInfos = type.GetMethods(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly);

                foreach( var methodInfo in staticMethodInfos )
                {
                    if( !methodInfo.IsSpecialName )
                    {
                        WriteMethod(type, true, methodInfo);
                    }
                    else if( methodInfo.Name.Substring(0, 3) == "op_" )
                    {
                        WriteOperator( type, methodInfo );
                    }

                }

                foreach( var methodInfo in instanceMethodInfos )
                {
                    if( !methodInfo.IsSpecialName )
                    {
                        WriteMethod(type, false, methodInfo);
                    }
                }

                System.Reflection.PropertyInfo[] properties = type.GetProperties(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly);

                foreach( var property in properties )
                {
                    var accessors = property.GetAccessors();
                    var setMethod = Array.Find<MethodInfo>( accessors, MatchNameSet );
                    WriteProperty( type, property, setMethod != null );
                }

                System.Reflection.EventInfo[] events = type.GetEvents(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly);

                foreach( var ev in events )
                {
                    WriteEvent( type, ev.Name );
                }

                var nestedTypes = type.GetNestedTypes(BindingFlags.Public|BindingFlags.DeclaredOnly);
                foreach( var subtype in nestedTypes)
                {
                    if( subtype.GetTypeInfo().IsEnum )
                    {
                        WriteEnum( type, subtype );
                    }
                }
            }
        }

        public void WriteTypes()
        {
            System.Reflection.Assembly nuiLib = typeof(Tizen.NUI.Position).GetTypeInfo().Assembly;

            foreach( var type in nuiLib.GetExportedTypes())
            {
                WriteType(type);
            }
        }

        private static bool MatchNameSet(MethodInfo mi)
        {
            return mi.Name.Substring(0,3) == "set";
        }

        [STAThread]
        static void Main(string[] args)
        {
            //var type = typeof( Tizen.NUI.UIComponents.Popup.OutsideTouchedEventArgs );
            //TypeHelper.DumpType(type);
            TypeHelper typeHelper = new TypeHelper();
            typeHelper.WriteTypes();
        }
    }
}
