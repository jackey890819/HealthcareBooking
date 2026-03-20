using System.ComponentModel;
using HealthcareBooking.API.Attributes;
using HealthcareBooking.API.DTOs;
using HealthcareBooking.Core.Entities;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Text.Json.Nodes;

namespace HealthcareBooking.API.Extensions;

public static class OpenApiOptionsExtensions
{
    public static OpenApiOptions AddHealthcareBookingMetadata(this OpenApiOptions options)
    {
        options.AddDocumentTransformer((document, _, _) =>
        {
            document.Info.Title = "Healthcare Booking API";
            document.Info.Description = "提供醫生、病患、門診、預約與掛號相關功能的 REST API。可透過 CRUD 端點管理主資料，或使用掛號端點直接建立預約。";
            return Task.CompletedTask;
        });

        options.AddSchemaTransformer((schema, context, _) =>
        {
            var type = context.JsonTypeInfo.Type;

            ApplyTypeDescription(schema, type);
            ApplyTypeExample(schema, type);

            //if (type == typeof(IdResponse))
            //{
            //    schema.Description = "建立成功後回傳的新資料識別碼。";
            //    schema.Example = new JsonObject
            //    {
            //        ["id"] = 101
            //    };
            //}
            //else if (type == typeof(Doctor))
            //{
            //    schema.Example = new JsonObject
            //    {
            //        ["id"] = 1,
            //        ["name"] = "王小明醫師"
            //    };
            //}
            //else if (type == typeof(Patient))
            //{
            //    schema.Example = new JsonObject
            //    {
            //        ["id"] = 1,
            //        ["name"] = "陳美玲"
            //    };
            //}
            //else if (type == typeof(Clinic))
            //{
            //    schema.Example = new JsonObject
            //    {
            //        ["id"] = 3,
            //        ["doctorId"] = 1,
            //        ["clinicDate"] = "2026-03-25T09:00:00+08:00",
            //        ["maxQuota"] = 20,
            //        ["currentBooked"] = 5,
            //        ["rowVersion"] = new JsonArray
            //        {
            //            1,
            //            2,
            //            3
            //        }
            //    };
            //}
            //else if (type == typeof(Appointment))
            //{
            //    schema.Example = new JsonObject
            //    {
            //        ["id"] = 12,
            //        ["appointmentDate"] = "2026-03-25T09:15:00+08:00",
            //        ["patientId"] = 1,
            //        ["clinicId"] = 3
            //    };
            //}

            return Task.CompletedTask;
        });

        return options;
    }

    private static void ApplyTypeDescription(OpenApiSchema schema, Type type)
    {
        var descriptionAttribute = type.GetCustomAttributes(typeof(DescriptionAttribute), inherit: false)
            .OfType<DescriptionAttribute>()
            .FirstOrDefault();

        if (descriptionAttribute is not null)
        {
            schema.Description = descriptionAttribute.Description;
        }
    }

    private static void ApplyTypeExample(OpenApiSchema schema, Type type)
    {
        var exampleAttribute = type.GetCustomAttributes(typeof(OpenApiExampleAttribute), inherit: false)
            .OfType<OpenApiExampleAttribute>()
            .FirstOrDefault();

        if (exampleAttribute is not null)
        {
            schema.Example = exampleAttribute.ToJsonNode();
        }
    }
}
