using APICatalogo.Context;
using APICatalogo.Domains;
using APICatalogo.Filters;
using APICatalogo.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly IRepository<Categoria> _categoriaRepository;
    private readonly ILogger _logger;
    public CategoriasController(IRepository<Categoria> categoriaRepository, ILogger<CategoriasController> logger)
    {
        _categoriaRepository =  categoriaRepository;
        _logger = logger;
    }
    
    
    [HttpGet]
    public ActionResult<IEnumerable<Categoria>> Get()
    {
        var categorias = _categoriaRepository.GetAll();
        return Ok(categorias);
    }
    
    [HttpGet("{id:int}", Name="ObterCategoria")]
    public ActionResult<Categoria> Get(int id)
    {
        var categoria = _categoriaRepository.Get(c => c.CategoriaId == id);
        if (categoria == null)
        {
            return NotFound("Categoria não encontrada...");
        }
        
        return Ok(categoria);
    }

    [HttpPost]
    public ActionResult<Categoria> Post(Categoria categoria)
    {
        if (categoria is null)
        {
            _logger.LogWarning($"Dados inválidos");
            return BadRequest("Dados inválidos");
        }
        
        var categoriaCriada = _categoriaRepository.Create(categoria);
        return new CreatedAtRouteResult("ObterCategoria", new { id = categoriaCriada.CategoriaId }, categoriaCriada);
    }

    [HttpPut("{id:int}")]
    public ActionResult<Categoria> Put(int id, Categoria categoria)
    {
        if (id != categoria.CategoriaId)
        {
            return BadRequest();
        }

        _categoriaRepository.Update(categoria);
        return Ok(categoria);
    }

    [HttpDelete("{id:int}")]
    public ActionResult Delete(int id)
    {
        var categoria = _categoriaRepository.Get(c => c.CategoriaId == id);
        
        if (categoria == null)
        {   
            _logger.LogWarning($"Categoria com id={id} não encontrada");
            return NotFound("Categoria não localizada...");
        }

        var categoriaExcluida = _categoriaRepository.Delete(categoria);
        return Ok(categoriaExcluida);
    }
}