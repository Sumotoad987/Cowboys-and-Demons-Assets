using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Owlcat.QA.Validation;
using HarmonyLib;
using System.Reflection;

namespace MyOwlcatModification
{
    public static class ValidationContextEx
    {
        [StringFormatMethod("errorFormat")]
        public static void AddError(this ValidationContext instance, string type, string designers, string errorFormat, params object[] args)
        {
            ValidationStack<IValidated> blah = AccessTools.Field(typeof (ValidationContext), "m_ValidationStack").GetValue(instance) as ValidationStack<IValidated>;
            AccessTools.DeclaredMethod(typeof(ValidationContext), "AddErrorInternal").Invoke(instance, new object[] { ErrorLevel.Unprioritized, designers, type, true, null, errorFormat + "." + blah.FormatValidationStack(), args });
        }

    }
}
