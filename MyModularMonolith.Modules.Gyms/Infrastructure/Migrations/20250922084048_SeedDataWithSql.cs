using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MyModularMonolith.Modules.Gyms.Infrastructure.Migrations;

/// <inheritdoc />
public partial class SeedDataWithSql : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Seed Products first
        migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [Gyms].[Products] WHERE Id = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa')
                INSERT INTO [Gyms].[Products] (Id, Name, Description, BasePrice, RequiresSchedule, RequiresInstructor, HasCapacityLimits, IsActive, CreatedAt)
                VALUES 
                    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Acceso Sala de Musculación', 'Acceso completo a la sala de musculación con equipos de última generación', 35.00, 0, 0, 1, 1, '2024-01-01T00:00:00.000Z'),
                    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Clase de Spinning', 'Clase de spinning de alta intensidad con música motivadora', 15.00, 1, 1, 1, 1, '2024-01-01T00:00:00.000Z'),
                    ('cccccccc-cccc-cccc-cccc-cccccccccccc', 'Entrenamiento Personal', 'Sesión de entrenamiento personalizado con instructor especializado', 45.00, 1, 1, 0, 1, '2024-01-01T00:00:00.000Z'),
                    ('dddddddd-dddd-dddd-dddd-dddddddddddd', 'Consulta Nutricional', 'Consulta personalizada con nutricionista especializado en deportes', 25.00, 1, 1, 0, 1, '2024-01-01T00:00:00.000Z'),
                    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', 'Clase de Zumba', 'Clase de baile fitness con ritmos latinos para todos los niveles', 12.00, 1, 1, 1, 1, '2024-01-01T00:00:00.000Z')
            ");

        // Seed GymProducts second
        migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [Gyms].[GymProducts] WHERE Id = '11111111-1111-1111-aaaa-111111111111')
                INSERT INTO [Gyms].[GymProducts] (Id, GymId, ProductId, Price, DiscountPercentage, IsActive, Schedule, MinCapacity, MaxCapacity, InstructorName, CreatedAt)
                VALUES 
                    ('11111111-1111-1111-aaaa-111111111111', '33333333-3333-3333-3333-333333333333', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 35.00, NULL, 1, '{}', NULL, 50, NULL, '2024-01-01T00:00:00.000Z'),
                    ('11111111-1111-1111-bbbb-111111111111', '33333333-3333-3333-3333-333333333333', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 15.00, NULL, 1, '{""monday"":[""19:00-20:00""],""wednesday"":[""19:00-20:00""],""friday"":[""19:00-20:00""]}', 5, 20, 'Carlos Rodríguez', '2024-01-01T00:00:00.000Z'),
                    ('22222222-2222-2222-aaaa-222222222222', '44444444-4444-4444-4444-444444444444', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 30.00, 10.0, 1, '{}', NULL, 40, NULL, '2024-01-01T00:00:00.000Z'),
                    ('22222222-2222-2222-eeee-222222222222', '44444444-4444-4444-4444-444444444444', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', 12.00, NULL, 1, '{""tuesday"":[""18:30-19:30""],""thursday"":[""18:30-19:30""],""saturday"":[""10:00-11:00""]}', 8, 25, 'María González', '2024-01-01T00:00:00.000Z')
            ");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Clean up in reverse order
        migrationBuilder.Sql("DELETE FROM [Gyms].[GymProducts] WHERE Id IN ('11111111-1111-1111-aaaa-111111111111', '11111111-1111-1111-bbbb-111111111111', '22222222-2222-2222-aaaa-222222222222', '22222222-2222-2222-eeee-222222222222')");

        migrationBuilder.Sql("DELETE FROM [Gyms].[Products] WHERE Id IN ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'dddddddd-dddd-dddd-dddd-dddddddddddd', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee')");
    }
}
