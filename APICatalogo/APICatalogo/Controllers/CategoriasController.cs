using APICatalogo.Context;
using APICatalogo.Domains;
using APICatalogo.DTOs;
using APICatalogo.DTOs.Mappings;
using APICatalogo.Filters;
using APICatalogo.Pagination;
using APICatalogo.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace APICatalogo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    // private readonly IRepository<Categoria> _categoriaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;
    
    public CategoriasController(IUnitOfWork unitOfWork, ILogger<CategoriasController> logger,  IMapper mapper)
    {
        // _categoriaRepository =  categoriaRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
    }

    [HttpGet("pagination")]
    public ActionResult<CategoriaDTO> Get([FromQuery] CategoriasParameters categoriasParameters)
    {
        var categorias = _unitOfWork.CategoriaRepository.GetCategorias(categoriasParameters);

        var metadata = new
        {
            categorias.TotalCount,
            categorias.PageSize,
            categorias.CurrentPage,
            categorias.TotalPages,
            categorias.HasNext,
            categorias.HasPrevious
        };
        
        Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));
        
        var categoriasDto = _mapper.Map<IEnumerable<Categoria>>(categorias);
        
        return Ok(categoriasDto);
    }
    
    
    [HttpGet]
    public ActionResult<IEnumerable<CategoriaDTO>> Get()
    {
        var categorias = _unitOfWork.CategoriaRepository.GetAll();

        if (categorias is null)
        {
            return NotFound("Não existem categorias cadastradas!");
        }

        // var categoriasDto = categorias.ToCategoriaDTOList();
        var categoriasDto = _mapper.Map<IEnumerable<CategoriaDTO>>(categorias);
        
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

        // var categoriaDto = categoria.ToCategoriaDto();
        var categoriaDto = _mapper.Map<CategoriaDTO>(categoria);
        
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

        // var categoria = categoriaDto.ToCategoria();
        var categoria = _mapper.Map<Categoria>(categoriaDto);
        
        var novaCategoriaCriada = _unitOfWork.CategoriaRepository.Create(categoria);
        _unitOfWork.Commit();

        // var novaCategoriaDto = novaCategoriaCriada.ToCategoriaDto();
        var novaCategoriaDto = _mapper.Map<CategoriaDTO>(novaCategoriaCriada);
        
        return new CreatedAtRouteResult("ObterCategoria", new { id = novaCategoriaDto.CategoriaId }, novaCategoriaDto);
    }

    [HttpPut("{id:int}")]
    public ActionResult<CategoriaDTO> Put(int id, CategoriaDTO categoriaDto)
    {
        if (id != categoriaDto.CategoriaId)
        {
            return BadRequest();
        }

        // var categoria = categoriaDto.ToCategoria();
        var categoria = _mapper.Map<Categoria>(categoriaDto);

        var categoriaAtualizada = _unitOfWork.CategoriaRepository.Update(categoria);
        _unitOfWork.Commit();

        // var categoriaAtualizadaDto = categoriaAtualizada.ToCategoriaDto();
        var categoriaAtualizadaDto =  _mapper.Map<CategoriaDTO>(categoriaAtualizada);
        
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

        // var categoriaExcluidaDto = categoriaExcluida.ToCategoriaDto();
        var categoriaExcluidaDto =  _mapper.Map<CategoriaDTO>(categoriaExcluida);
        
        return Ok(categoriaExcluidaDto);
    }
}