using APICatalogo.Context;
using APICatalogo.Domains;
using APICatalogo.Pagination;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.EF;

namespace APICatalogo.Repositories;

public class CategoriaRepository : Repository<Categoria>, ICategoriaRepository
{
    public CategoriaRepository(AppDbContext context) : base(context)
    {
    }
    
    public async Task<IPagedList<Categoria>> GetCategoriasAsync(CategoriasParameters categoriasParams)
    {
        var categoriasOrdenadas = GetAll().OrderBy(c => c.CategoriaId);

        var resultado = await categoriasOrdenadas.ToPagedListAsync(categoriasParams.pageNumber, categoriasParams.PageSize);

        return resultado;
    }
    
    public async Task<IPagedList<Categoria>> GetCategoriasFiltroNomeAsync(CategoriasFiltroNome categoriasParams)
    {
        var categorias = GetAll();

        if (!string.IsNullOrEmpty(categoriasParams.Nome))
        {
            categorias = categorias.Where(c => c.Nome.Contains(categoriasParams.Nome));
        }

        var categoriasFiltradas = await categorias.ToPagedListAsync(categoriasParams.pageNumber, categoriasParams.PageSize);
        
        return categoriasFiltradas;
    }
}