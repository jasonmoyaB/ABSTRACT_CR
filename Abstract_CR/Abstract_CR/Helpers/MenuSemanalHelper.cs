using Abstract_CR.Data;
using Abstract_CR.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace Abstract_CR.Helpers
{
    public class MenuSemanalHelper
    {
        private readonly ApplicationDbContext _context;

        public MenuSemanalHelper(ApplicationDbContext context)
        {
            _context = context;
        }

        // Función helper para calcular el lunes de la semana actual
        private DateTime CalcularLunesSemana(DateTime fecha)
        {
            int diasHastaLunes = ((int)DayOfWeek.Monday - (int)fecha.DayOfWeek + 7) % 7;
            if (diasHastaLunes == 0 && fecha.DayOfWeek != DayOfWeek.Monday)
                diasHastaLunes = 7;
            return fecha.AddDays(-diasHastaLunes).Date;
        }

        public MenuSemanal? ObtenerMenuPorDia(string diaSemana)
        {
            // Obtener el menú de la semana actual para ese día
            var lunesSemanaActual = CalcularLunesSemana(DateTime.Today);
            return _context.MenuSemanal
                .Where(m => m.DiaSemana == diaSemana && m.SemanaDel == lunesSemanaActual)
                .OrderByDescending(m => m.SemanaDel)
                .FirstOrDefault();
        }

        public List<MenuSemanal> ObtenerTodosLosMenus()
        {
            // Obtener los menús de la semana actual (lunes a domingo)
            var lunesSemanaActual = CalcularLunesSemana(DateTime.Today);
            var domingoSemanaActual = lunesSemanaActual.AddDays(6);
            
            return _context.MenuSemanal
                .Where(m => m.SemanaDel == lunesSemanaActual)
                .OrderBy(m => m.DiaSemana)
                .ToList();
        }

        public MenuSemanalViewModel? ObtenerMenuViewModelPorDia(string diaSemana)
        {
            var menu = ObtenerMenuPorDia(diaSemana);
            if (menu == null) return null;

            return ConvertirAViewModel(menu);
        }

        public List<MenuSemanalViewModel> ObtenerTodosLosMenusViewModel()
        {
            var menus = ObtenerTodosLosMenus();
            return menus.Select(ConvertirAViewModel).ToList();
        }

        public bool GuardarMenu(MenuSemanalViewModel viewModel, string? rutaImagen = null)
        {
            try
            {
                MenuSemanal menu;

                // Obtener o crear TipoMenuID por defecto
                int tipoMenuID = 1;
                
                // Crear TipoMenu si no existe usando el stored procedure
                try
                {
                    _context.Database.ExecuteSqlRaw("EXEC spTipoMenu_Upsert @Nombre = 'Menú Semanal', @Descripcion = 'Menú semanal del chef'");
                    
                    // Obtener el TipoMenuID recién creado o existente
                    var connection = _context.Database.GetDbConnection();
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT TOP 1 TipoMenuID FROM TiposMenu WHERE Nombre = 'Menú Semanal' ORDER BY TipoMenuID";
                        var result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            tipoMenuID = Convert.ToInt32(result);
                        }
                    }
                    connection.Close();
                }
                catch (Exception)
                {
                    // Si falla, intentar obtener cualquier TipoMenuID disponible
                    try
                    {
                        var connection = _context.Database.GetDbConnection();
                        connection.Open();
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT TOP 1 TipoMenuID FROM TiposMenu ORDER BY TipoMenuID";
                            var result = command.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                tipoMenuID = Convert.ToInt32(result);
                            }
                        }
                        connection.Close();
                    }
                    catch
                    {
                        // Si aún falla, usar ID 1 (asumiendo que existe o se creará manualmente)
                        tipoMenuID = 1;
                    }
                }

                // Calcular el lunes de la semana actual
                var lunesSemanaActual = CalcularLunesSemana(DateTime.Today);

                // Buscar un menú existente para este día específico de la semana
                // Esto permite tener un platillo diferente para cada día
                var menuExistente = _context.MenuSemanal
                    .FirstOrDefault(m => m.TipoMenuID == tipoMenuID 
                        && m.SemanaDel == lunesSemanaActual 
                        && m.DiaSemana == viewModel.DiaSemana);

                if (viewModel.MenuSemanalID.HasValue && viewModel.MenuSemanalID.Value > 0)
                {
                    // Si se proporciona un ID, buscar por ID primero
                    var menuPorId = _context.MenuSemanal.Find(viewModel.MenuSemanalID.Value);
                    if (menuPorId != null)
                    {
                        menu = menuPorId;
                    }
                    else if (menuExistente != null)
                    {
                        // Si no existe por ID pero existe para este día, usar ese
                        menu = menuExistente;
                    }
                    else
                    {
                        // No se encontró, crear uno nuevo
                        menu = new MenuSemanal
                        {
                            TipoMenuID = tipoMenuID,
                            SemanaDel = lunesSemanaActual,
                            DiaSemana = viewModel.DiaSemana
                        };
                        _context.MenuSemanal.Add(menu);
                    }
                }
                else if (menuExistente != null)
                {
                    // Existe un menú para este día, actualizarlo
                    menu = menuExistente;
                }
                else
                {
                    // No existe ninguno para este día, crear uno nuevo
                    menu = new MenuSemanal
                    {
                        TipoMenuID = tipoMenuID,
                        SemanaDel = lunesSemanaActual,
                        DiaSemana = viewModel.DiaSemana
                    };
                    _context.MenuSemanal.Add(menu);
                }

                // Actualizar todos los campos del menú (ya sea existente o encontrado por ID)
                menu.TipoMenuID = tipoMenuID;
                menu.SemanaDel = lunesSemanaActual;
                menu.NombrePlatillo = viewModel.NombrePlatillo;
                menu.DiaSemana = viewModel.DiaSemana;
                menu.Caracteristicas = (viewModel.Caracteristicas != null && viewModel.Caracteristicas.Any()) 
                    ? JsonSerializer.Serialize(viewModel.Caracteristicas.Where(c => !string.IsNullOrWhiteSpace(c)).ToList()) 
                    : null;
                menu.IngredientesPrincipales = (viewModel.IngredientesPrincipales != null && viewModel.IngredientesPrincipales.Any()) 
                    ? JsonSerializer.Serialize(viewModel.IngredientesPrincipales.Where(i => !string.IsNullOrWhiteSpace(i)).ToList()) 
                    : null;
                menu.TipChef = string.IsNullOrWhiteSpace(viewModel.TipChef) ? null : viewModel.TipChef;
                menu.Descripcion = string.IsNullOrWhiteSpace(viewModel.Descripcion) ? null : viewModel.Descripcion;

                if (!string.IsNullOrEmpty(rutaImagen))
                {
                    menu.RutaImagen = rutaImagen;
                }

                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                // Log del error para debugging
                var errorMessage = $"Error al guardar menú: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" | InnerException: {ex.InnerException.Message}";
                }
                
                // Si es un error de base de datos, incluir más detalles
                if (ex is Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
                {
                    errorMessage += $" | DB Error: {dbEx.Message}";
                }
                
                System.Diagnostics.Debug.WriteLine(errorMessage);
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                
                // Lanzar la excepción para que el controlador pueda capturarla y mostrar el mensaje
                throw;
            }
        }

        private MenuSemanalViewModel ConvertirAViewModel(MenuSemanal menu)
        {
            var viewModel = new MenuSemanalViewModel
            {
                MenuSemanalID = menu.MenuSemanalID,
                NombrePlatillo = menu.NombrePlatillo,
                DiaSemana = menu.DiaSemana,
                TipChef = menu.TipChef,
                RutaImagen = menu.RutaImagen,
                Descripcion = menu.Descripcion,
                Activo = true // Siempre activo para la tabla existente
            };

            // Deserializar características - manejar casos de doble serialización
            if (!string.IsNullOrEmpty(menu.Caracteristicas))
            {
                try
                {
                    var deserializado = JsonSerializer.Deserialize<List<string>>(menu.Caracteristicas);
                    if (deserializado != null)
                    {
                        // Si algún elemento es un JSON string, deserializarlo también
                        viewModel.Caracteristicas = deserializado.Where(c => c != null).Select(c =>
                        {
                            if (c.StartsWith("[") && c.EndsWith("]"))
                            {
                                try
                                {
                                    var subList = JsonSerializer.Deserialize<List<string>>(c);
                                    return subList ?? new List<string> { c };
                                }
                                catch
                                {
                                    return new List<string> { c };
                                }
                            }
                            return new List<string> { c };
                        }).SelectMany(x => x).Where(x => !string.IsNullOrWhiteSpace(x) && x != "[]" && x != "null").ToList();
                    }
                    else
                    {
                        viewModel.Caracteristicas = new List<string>();
                    }
                }
                catch
                {
                    // Si falla, intentar como string simple separado por comas
                    viewModel.Caracteristicas = menu.Caracteristicas.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(c => c.Trim())
                        .Where(c => !string.IsNullOrWhiteSpace(c) && c != "[]" && c != "null")
                        .ToList();
                }
            }

            // Deserializar ingredientes - manejar casos de doble serialización
            if (!string.IsNullOrEmpty(menu.IngredientesPrincipales))
            {
                try
                {
                    var deserializado = JsonSerializer.Deserialize<List<string>>(menu.IngredientesPrincipales);
                    if (deserializado != null)
                    {
                        // Si algún elemento es un JSON string, deserializarlo también
                        viewModel.IngredientesPrincipales = deserializado.Where(i => i != null).Select(i =>
                        {
                            if (i.StartsWith("[") && i.EndsWith("]"))
                            {
                                try
                                {
                                    var subList = JsonSerializer.Deserialize<List<string>>(i);
                                    return subList ?? new List<string> { i };
                                }
                                catch
                                {
                                    return new List<string> { i };
                                }
                            }
                            return new List<string> { i };
                        }).SelectMany(x => x).Where(x => !string.IsNullOrWhiteSpace(x) && x != "[]" && x != "null").ToList();
                    }
                    else
                    {
                        viewModel.IngredientesPrincipales = new List<string>();
                    }
                }
                catch
                {
                    // Si falla, intentar como string simple separado por comas
                    viewModel.IngredientesPrincipales = menu.IngredientesPrincipales.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(i => i.Trim())
                        .Where(i => !string.IsNullOrWhiteSpace(i) && i != "[]" && i != "null")
                        .ToList();
                }
            }

            return viewModel;
        }
    }
}

