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

        public MenuSemanal? ObtenerMenuPorDia(string diaSemana)
        {
            // Obtener el menú más reciente para ese día
            return _context.MenuSemanal
                .Where(m => m.DiaSemana == diaSemana)
                .OrderByDescending(m => m.SemanaDel)
                .FirstOrDefault();
        }

        public List<MenuSemanal> ObtenerTodosLosMenus()
        {
            // Obtener los menús de la semana actual
            var inicioSemana = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            return _context.MenuSemanal
                .Where(m => m.SemanaDel >= inicioSemana)
                .OrderBy(m => m.DiaSemana)
                .ThenBy(m => m.SemanaDel)
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
                catch (Exception ex)
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

                if (viewModel.MenuSemanalID.HasValue && viewModel.MenuSemanalID.Value > 0)
                {
                    // Actualizar existente
                    var menuExistente = _context.MenuSemanal.Find(viewModel.MenuSemanalID.Value);
                    if (menuExistente == null) return false;
                    menu = menuExistente;

                    menu.NombrePlatillo = viewModel.NombrePlatillo;
                    menu.DiaSemana = viewModel.DiaSemana;
                    menu.Caracteristicas = JsonSerializer.Serialize(viewModel.Caracteristicas);
                    menu.IngredientesPrincipales = JsonSerializer.Serialize(viewModel.IngredientesPrincipales);
                    menu.TipChef = viewModel.TipChef;
                    menu.Descripcion = viewModel.Descripcion;

                    if (!string.IsNullOrEmpty(rutaImagen))
                    {
                        menu.RutaImagen = rutaImagen;
                    }
                }
                else
                {
                    // Crear nuevo - calcular inicio de semana actual
                    var inicioSemana = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                    
                    menu = new MenuSemanal
                    {
                        TipoMenuID = tipoMenuID,
                        SemanaDel = inicioSemana,
                        NombrePlatillo = viewModel.NombrePlatillo,
                        DiaSemana = viewModel.DiaSemana,
                        Caracteristicas = JsonSerializer.Serialize(viewModel.Caracteristicas),
                        IngredientesPrincipales = JsonSerializer.Serialize(viewModel.IngredientesPrincipales),
                        TipChef = viewModel.TipChef,
                        Descripcion = viewModel.Descripcion,
                        RutaImagen = rutaImagen
                    };

                    _context.MenuSemanal.Add(menu);
                }

                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                // Log del error para debugging
                System.Diagnostics.Debug.WriteLine($"Error al guardar menú: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"InnerException: {ex.InnerException.Message}");
                }
                return false;
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

            // Deserializar características
            if (!string.IsNullOrEmpty(menu.Caracteristicas))
            {
                try
                {
                    viewModel.Caracteristicas = JsonSerializer.Deserialize<List<string>>(menu.Caracteristicas) ?? new List<string>();
                }
                catch
                {
                    viewModel.Caracteristicas = new List<string>();
                }
            }

            // Deserializar ingredientes
            if (!string.IsNullOrEmpty(menu.IngredientesPrincipales))
            {
                try
                {
                    viewModel.IngredientesPrincipales = JsonSerializer.Deserialize<List<string>>(menu.IngredientesPrincipales) ?? new List<string>();
                }
                catch
                {
                    viewModel.IngredientesPrincipales = new List<string>();
                }
            }

            return viewModel;
        }
    }
}

