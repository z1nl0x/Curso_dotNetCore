using APICatalogo.Context;
using APICatalogo.Domains;
using APICatalogo.DTOs;
using APICatalogo.DTOs.Mappings;
using APICatalogo.Filters;
using APICatalogo.Pagination;
using APICatalogo.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using X.PagedList;

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
    
    private ActionResult<CategoriaDTO> ObterCategorias(IPagedList<Categoria> categorias)
    {
        var metadata = new
        {
            categorias.Count,
            categorias.PageSize,
            categorias.PageCount,
            categorias.TotalItemCount,
            categorias.PageNumber,
            categorias.HasNextPage,
            categorias.HasPreviousPage
        };
        
        Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));
        
        var categoriasDto = _mapper.Map<IEnumerable<Categoria>>(categorias);
        
        return Ok(categoriasDto);
    }

    [HttpGet("pagination")]
    public async Task<ActionResult<CategoriaDTO>> Get([FromQuery] CategoriasParameters categoriasParameters)
    {
        var categorias = await _unitOfWork.CategoriaRepository.GetCategoriasAsync(categoriasParameters);

        return ObterCategorias(categorias);
    }

    [HttpGet("filter/nome/pagination")]
    public async Task<ActionResult<CategoriaDTO>> GetCategoriasFiltradas([FromQuery] CategoriasFiltroNome categoriasFiltro)
    {
        var categoriasFiltradas = await _unitOfWork.CategoriaRepository.GetCategoriasFiltroNomeAsync(categoriasFiltro);
        
        return ObterCategorias(categoriasFiltradas);
    }
    
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> Get()
    {
        var categorias = await _unitOfWork.CategoriaRepository.GetAllAsync();

        if (categorias is null)
        {
            return NotFound("Não existem categorias cadastradas!");
        }

        // var categoriasDto = categorias.ToCategoriaDTOList();
        var categoriasDto = _mapper.Map<IEnumerable<CategoriaDTO>>(categorias);
        
        return Ok(categoriasDto);
    }
    
    [HttpGet("{id:int}", Name="ObterCategoria")]
    public async Task<ActionResult<CategoriaDTO>> Get(int id)
    {
        var categoria = await _unitOfWork.CategoriaRepository.GetAsync(c => c.CategoriaId == id);
        if (categoria == null)
        {
            return NotFound("Categoria não encontrada...");
        }

        // var categoriaDto = categoria.ToCategoriaDto();
        var categoriaDto = _mapper.Map<CategoriaDTO>(categoria);
        
        return Ok(categoriaDto);
    }

    [HttpPost]
    public async Task<ActionResult<CategoriaDTO>> Post(CategoriaDTO categoriaDto)
    {
        if (categoriaDto is null)
        {
            _logger.LogWarning($"Dados inválidos");
            return BadRequest("Dados inválidos");
        }

        // var categoria = categoriaDto.ToCategoria();
        var categoria = _mapper.Map<Categoria>(categoriaDto);
        
        var novaCategoriaCriada = _unitOfWork.CategoriaRepository.Create(categoria);
        await _unitOfWork.CommitAsync();

        // var novaCategoriaDto = novaCategoriaCriada.ToCategoriaDto();
        var novaCategoriaDto = _mapper.Map<CategoriaDTO>(novaCategoriaCriada);
        
        return new CreatedAtRouteResult("ObterCategoria", new { id = novaCategoriaDto.CategoriaId }, novaCategoriaDto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoriaDTO>> Put(int id, CategoriaDTO categoriaDto)
    {
        if (id != categoriaDto.CategoriaId)
        {
            return BadRequest();
        }

        // var categoria = categoriaDto.ToCategoria();
        var categoria = _mapper.Map<Categoria>(categoriaDto);

        var categoriaAtualizada = _unitOfWork.CategoriaRepository.Update(categoria);
        await _unitOfWork.CommitAsync();

        // var categoriaAtualizadaDto = categoriaAtualizada.ToCategoriaDto();
        var categoriaAtualizadaDto =  _mapper.Map<CategoriaDTO>(categoriaAtualizada);
        
        return Ok(categoriaAtualizadaDto);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<CategoriaDTO>> Delete(int id)
    {
        var categoria = await _unitOfWork.CategoriaRepository.GetAsync(c => c.CategoriaId == id);
        
        
        if (categoria == null)
        {
            _logger.LogWarning($"Categoria com id={id} não encontrada");
            return NotFound("Categoria não localizada...");
        }

        var categoriaExcluida = _unitOfWork.CategoriaRepository.Delete(categoria);
        await _unitOfWork.CommitAsync();

        // var categoriaExcluidaDto = categoriaExcluida.ToCategoriaDto();
        var categoriaExcluidaDto =  _mapper.Map<CategoriaDTO>(categoriaExcluida);
        
        return Ok(categoriaExcluidaDto);
    }
}