using APICatalogo.Context;
using APICatalogo.Domains;
using APICatalogo.Pagination;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo.Repositories;

public class CategoriaRepository : Repository<Categoria>, ICategoriaRepository
{
    public CategoriaRepository(AppDbContext context) : base(context)
    {
    }
    
    public PagedList<Categoria> GetCategorias(CategoriasParameters categoriasParams)
    {
        var categorias = GetAll().OrderBy(categoria => categoria.CategoriaId).AsQueryable();
        var categoriasOrdenadas = PagedList<Categoria>.ToPagedList(categorias, categoriasParams.pageNumber, categoriasParams.PageSize);
        return categoriasOrdenadas;
    }
}