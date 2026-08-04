using APICatalogo.Context;
using APICatalogo.Domains;
using APICatalogo.DTOs;
using APICatalogo.DTOs.Mappings;
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
    public ActionResult<IEnumerable<CategoriaDTO>> Get()
    {
        var categorias = _unitOfWork.CategoriaRepository.GetAll();

        if (categorias is null)
        {
            return NotFound("Não existem categorias cadastradas!");
        }

        var categoriasDto = categorias.ToCategoriaDTOList();
        
        return Ok(categoriasDto);
    }
    
    [HttpGet("{id:int}", Name="ObterCategoria")]
    public ActionResult<CategoriaDTO> Get(int id)
    {
        var categoria = _unitOfWork.CategoriaRepository.Get(c => c.CategoriaId == id);
        if (categoria == null)
        {
            return NotFound("Categoria não encontrada...");
        }

        var categoriaDto = categoria.ToCategoriaDto();
        
        return Ok(categoriaDto);
    }

    [HttpPost]
    public ActionResult<CategoriaDTO> Post(CategoriaDTO categoriaDto)
    {
        if (categoriaDto is null)
        {
            _logger.LogWarning($"Dados inválidos");
            return BadRequest("Dados inválidos");
        }

        var categoria = categoriaDto.ToCategoria();
        
        var novaCategoriaCriada = _unitOfWork.CategoriaRepository.Create(categoria);
        _unitOfWork.Commit();

        var novaCategoriaDto = novaCategoriaCriada.ToCategoriaDto();
        
        return new CreatedAtRouteResult("ObterCategoria", new { id = novaCategoriaDto.CategoriaId }, novaCategoriaDto);
    }

    [HttpPut("{id:int}")]
    public ActionResult<CategoriaDTO> Put(int id, CategoriaDTO categoriaDto)
    {
        if (id != categoriaDto.CategoriaId)
        {
            return BadRequest();
        }

        var categoria = categoriaDto.ToCategoria();

        var categoriaAtualizada = _unitOfWork.CategoriaRepository.Update(categoria);
        _unitOfWork.Commit();

        var categoriaAtualizadaDto = categoriaAtualizada.ToCategoriaDto();
        
        return Ok(categoriaAtualizadaDto);
    }

    [HttpDelete("{id:int}")]
    public ActionResult<CategoriaDTO> Delete(int id)
    {
        var categoria = _unitOfWork.CategoriaRepository.Get(c => c.CategoriaId == id);
        
        
        if (categoria == null)
        {
            _logger.LogWarning($"Categoria com id={id} não encontrada");
            return NotFound("Categoria não localizada...");
        }

        var categoriaExcluida = _unitOfWork.CategoriaRepository.Delete(categoria);
        _unitOfWork.Commit();

        var categoriaExcluidaDto = categoriaExcluida.ToCategoriaDto();
        
        return Ok(categoriaExcluidaDto);
    }
}