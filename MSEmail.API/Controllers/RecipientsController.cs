using MediatR;
using Microsoft.AspNetCore.Mvc;
using MSEmail.Application.Commands.Recipients;
using MSEmail.Application.DTOs;
using MSEmail.Application.Queries.Recipients;

namespace MSEmail.API.Controllers;

/// <summary>
/// Controller para gerenciamento de destinatários de e-mail
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RecipientsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RecipientsController> _logger;

    public RecipientsController(IMediator mediator, ILogger<RecipientsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtém todos os destinatários
    /// </summary>
    /// <param name="onlyActive">Se true, retorna apenas destinatários ativos</param>
    /// <returns>Lista de destinatários</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RecipientDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RecipientDto>>> GetAllAsync([FromQuery] bool onlyActive = false)
    {
        _logger.LogInformation("Obtendo todos os destinatários. OnlyActive: {OnlyActive}", onlyActive);

        var query = new GetAllRecipientsQuery(onlyActive);
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Obtém um destinatário por ID
    /// </summary>
    /// <param name="id">ID do destinatário</param>
    /// <returns>Destinatário</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RecipientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecipientDto>> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Obtendo destinatário por ID: {Id}", id);

        var query = new GetRecipientByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound($"Destinatário com ID '{id}' não encontrado.");
        }

        return Ok(result);
    }

    /// <summary>
    /// Obtém um destinatário por e-mail
    /// </summary>
    /// <param name="email">E-mail do destinatário</param>
    /// <returns>Destinatário</returns>
    [HttpGet("by-email/{email}")]
    [ProducesResponseType(typeof(RecipientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecipientDto>> GetByEmailAsync(string email)
    {
        _logger.LogInformation("Obtendo destinatário por e-mail: {Email}", email);

        var query = new GetRecipientByEmailQuery(email);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound($"Destinatário com e-mail '{email}' não encontrado.");
        }

        return Ok(result);
    }

    /// <summary>
    /// Cria um novo destinatário
    /// </summary>
    /// <param name="createDto">Dados do destinatário a ser criado</param>
    /// <returns>Destinatário criado</returns>
    [HttpPost]
    [ProducesResponseType(typeof(RecipientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RecipientDto>> CreateAsync([FromBody] CreateRecipientDto createDto)
    {
        _logger.LogInformation("Criando novo destinatário: {Email}", createDto.Email);

        try
        {
            var command = new CreateRecipientCommand(createDto);
            var result = await _mediator.Send(command);

            return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao criar destinatário");
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Atualiza um destinatário existente
    /// </summary>
    /// <param name="id">ID do destinatário</param>
    /// <param name="updateDto">Dados atualizados do destinatário</param>
    /// <returns>Destinatário atualizado</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RecipientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecipientDto>> UpdateAsync(Guid id, [FromBody] UpdateRecipientDto updateDto)
    {
        if (id != updateDto.Id)
        {
            return BadRequest("ID da URL não confere com o ID do corpo da requisição.");
        }

        _logger.LogInformation("Atualizando destinatário: {Id}", id);

        try
        {
            var command = new UpdateRecipientCommand(updateDto);
            var result = await _mediator.Send(command);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Destinatário não encontrado para atualização");
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao atualizar destinatário");
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Deleta um destinatário
    /// </summary>
    /// <param name="id">ID do destinatário</param>
    /// <returns>Resultado da operação</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deletando destinatário: {Id}", id);

        var command = new DeleteRecipientCommand(id);
        var result = await _mediator.Send(command);

        if (!result)
        {
            return NotFound($"Destinatário com ID '{id}' não encontrado.");
        }

        return NoContent();
    }
}
