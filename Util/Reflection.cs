/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;

namespace CupheadArchipelago.Util {
    public class Reflection {
        public static Type GetEnumeratorType(MethodBase enumerator) {
            if (enumerator == null) throw new ArgumentNullException($"{nameof(GetEnumeratorType)}: Argument cannot be null!");

            Type res = null;

            MethodDefinition enumDef = new DynamicMethodDefinition(enumerator).Definition;
            ILContext iLContext = new ILContext(enumDef);

            if (iLContext.Method.ReturnType.Name.StartsWith("UniTask")) {
                TypeReference first = iLContext.Body.Variables.FirstOrDefault()?.VariableType;

                if (first != null && !first.Name.Contains(enumerator.Name))
                {
                    Logging.LogWarning($"{nameof(GetEnumeratorType)}: First Var name invalid: {first.Name}");
                    return null;
                }

                res = first.ResolveReflection();
            }
            else {
                MethodReference ctor = null;
                ILCursor iLCursor = new ILCursor(iLContext);
                iLCursor.GotoNext((Instruction i) => i.MatchNewobj(out ctor));

                if (ctor == null || ctor.Name != ".ctor") {
                    Logging.LogWarning($"{nameof(GetEnumeratorType)}: Invalid enumerator ctor: {enumerator.FullDescription()}");
                }

                res = ctor.DeclaringType.ResolveReflection();
            }

            return res;
        }
    }
}