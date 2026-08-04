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
    // private readonly IRepository<Categoria> _categoriaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;
    
    public CategoriasController(IUnitOfWork unitOfWork, ILogger<CategoriasController> logger)
    {
        // _categoriaRepository =  categoriaRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    
    
    [HttpGet]
    public ActionResult<IEnumerable<Categoria>> Get()
    {
        var categorias = _unitOfWork.CategoriaRepository.GetAll();
        return Ok(categorias);
    }
    
    [HttpGet("{id:int}", Name="ObterCategoria")]
    public ActionResult<Categoria> Get(int id)
    {
        var categoria = _unitOfWork.CategoriaRepository.Get(c => c.CategoriaId == id);
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
        
        var categoriaCriada = _unitOfWork.CategoriaRepository.Create(categoria);
        _unitOfWork.Commit();
        
        return new CreatedAtRouteResult("ObterCategoria", new { id = categoriaCriada.CategoriaId }, categoriaCriada);
    }

    [HttpPut("{id:int}")]
    public ActionResult<Categoria> Put(int id, Categoria categoria)
    {
        if (id != categoria.CategoriaId)
        {
            return BadRequest();
        }

        _unitOfWork.CategoriaRepository.Update(categoria);
        _unitOfWork.Commit();
        
        return Ok(categoria);
    }

    [HttpDelete("{id:int}")]
    public ActionResult Delete(int id)
    {
        var categoria = _unitOfWork.CategoriaRepository.Get(c => c.CategoriaId == id);
        
        
        if (categoria == null)
        {   
            _logger.LogWarning($"Categoria com id={id} não encontrada");
            return NotFound("Categoria não localizada...");
        }

        var categoriaExcluida = _unitOfWork.CategoriaRepository.Delete(categoria);
        _unitOfWork.Commit();
        
        return Ok(categoriaExcluida);
    }
}