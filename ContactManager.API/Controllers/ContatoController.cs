using ContactManager.Application.DTOs;
using ContactManager.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ContactManager.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class ContatoController : ControllerBase
{
    private readonly IContatoService _contatoService;

    public ContatoController(IContatoService contatoService)
    {
        _contatoService = contatoService;
    }

    /// <summary>
    /// Lista todos os contatos ativos.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ContatoResponseDto<IEnumerable<ContatoDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var response = await _contatoService.GetAllActiveAsync(cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Retorna os detalhes de um contato ativo pelo ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ContatoResponseDto<ContatoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ContatoResponseDto<ContatoDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _contatoService.GetByIdAsync(id, cancellationToken);

        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// Cria um novo contato. O contato deve ter pelo menos 18 anos.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ContatoResponseDto<ContatoDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ContatoResponseDto<ContatoDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateContatoDto dto, CancellationToken cancellationToken)
    {
        var response = await _contatoService.CreateAsync(dto, cancellationToken);

        if (!response.Success)
            return BadRequest(response);

        return CreatedAtAction(nameof(GetById), new { id = response.Data!.Id }, response);
    }

    /// <summary>
    /// Atualiza um contato ativo.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ContatoResponseDto<ContatoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ContatoResponseDto<ContatoDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ContatoResponseDto<ContatoDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContatoDto dto, CancellationToken cancellationToken)
    {
        var response = await _contatoService.UpdateAsync(id, dto, cancellationToken);

        if (!response.Success)
        {
            if (response.Message.Contains("not found"))
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Ativa um contato inativo.
    /// </summary>
    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(typeof(ContatoResponseDto<ContatoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ContatoResponseDto<ContatoDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ContatoResponseDto<ContatoDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var response = await _contatoService.ActivateAsync(id, cancellationToken);

        if (!response.Success)
        {
            if (response.Message.Contains("not found"))
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Desativa um contato ativo.
    /// </summary>
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(ContatoResponseDto<ContatoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ContatoResponseDto<ContatoDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var response = await _contatoService.DeactivateAsync(id, cancellationToken);

        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// Exclui permanentemente um contato pelo ID.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ContatoResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ContatoResponseDto<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var response = await _contatoService.DeleteAsync(id, cancellationToken);

        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }
}
