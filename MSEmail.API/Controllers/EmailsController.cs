using MediatR;
using Microsoft.AspNetCore.Mvc;
using MSEmail.Application.Commands.Emails;
using MSEmail.Application.DTOs;
using MSEmail.Application.Queries.Emails;
using MSEmail.Domain.Entities;

namespace MSEmail.API.Controllers;

/// <summary>
/// Controller para gerenciamento de envio de e-mails
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EmailsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<EmailsController> _logger;

    public EmailsController(IMediator mediator, ILogger<EmailsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Envia e-mails usando um template para múltiplos destinatários
    /// </summary>
    /// <param name="request">Dados da requisição de envio</param>
    /// <returns>Resultado do envio</returns>
    [HttpPost("send")]
    [ProducesResponseType(typeof(SendEmailResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SendEmailResponseDto>> SendEmailsAsync([FromBody] SendEmailRequestDto request)
    {
        _logger.LogInformation("Iniciando envio de e-mails para {Count} destinatários", request.RecipientIds.Count);

        if (request.RecipientIds.Count == 0)
        {
            return BadRequest("Lista de destinatários não pode estar vazia.");
        }

        var command = new SendEmailCommand(request);
        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return BadRequest(result.Message);
        }

        return Ok(result);
    }

    /// <summary>
    /// Reprocessa um e-mail específico que falhou
    /// </summary>
    /// <param name="emailLogId">ID do log de e-mail</param>
    /// <returns>Resultado do reprocessamento</returns>
    [HttpPost("{emailLogId:guid}/reprocess")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReprocessEmailAsync(Guid emailLogId)
    {
        _logger.LogInformation("Reprocessando e-mail: {EmailLogId}", emailLogId);

        var command = new ReprocessEmailCommand(emailLogId);
        var result = await _mediator.Send(command);

        if (!result)
        {
            return NotFound($"Log de e-mail com ID '{emailLogId}' não encontrado.");
        }

        return Ok(new { message = "E-mail adicionado à fila para reprocessamento." });
    }

    /// <summary>
    /// Obtém um log de e-mail por ID
    /// </summary>
    /// <param name="id">ID do log de e-mail</param>
    /// <returns>Log de e-mail</returns>
    [HttpGet("logs/{id:guid}")]
    [ProducesResponseType(typeof(EmailLogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmailLogDto>> GetEmailLogAsync(Guid id)
    {
        _logger.LogInformation("Obtendo log de e-mail: {Id}", id);

        var query = new GetEmailLogByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound($"Log de e-mail com ID '{id}' não encontrado.");
        }

        return Ok(result);
    }

    /// <summary>
    /// Obtém logs de e-mail por status
    /// </summary>
    /// <param name="status">Status dos e-mails (Pending, Processing, Sent, Failed, Retrying)</param>
    /// <returns>Lista de logs de e-mail</returns>
    [HttpGet("logs/by-status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<EmailLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<EmailLogDto>>> GetEmailLogsByStatusAsync(string status)
    {
        _logger.LogInformation("Obtendo logs de e-mail por status: {Status}", status);

        if (!Enum.TryParse<EmailStatus>(status, true, out var emailStatus))
        {
            return BadRequest($"Status inválido: {status}. Valores válidos: Pending, Processing, Sent, Failed, Retrying");
        }

        var query = new GetEmailLogsByStatusQuery(emailStatus);
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Obtém logs de e-mail por destinatário
    /// </summary>
    /// <param name="recipientId">ID do destinatário</param>
    /// <returns>Lista de logs de e-mail</returns>
    [HttpGet("logs/by-recipient/{recipientId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<EmailLogDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EmailLogDto>>> GetEmailLogsByRecipientAsync(Guid recipientId)
    {
        _logger.LogInformation("Obtendo logs de e-mail por destinatário: {RecipientId}", recipientId);

        var query = new GetEmailLogsByRecipientQuery(recipientId);
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Obtém estatísticas de envio de e-mails
    /// </summary>
    /// <returns>Estatísticas de e-mails</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(EmailStatisticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<EmailStatisticsDto>> GetEmailStatisticsAsync()
    {
        _logger.LogInformation("Obtendo estatísticas de e-mails");

        var query = new GetEmailStatisticsQuery();
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Processa e-mails pendentes (para uso administrativo)
    /// </summary>
    /// <returns>Número de e-mails processados</returns>
    [HttpPost("process-pending")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ProcessPendingEmailsAsync()
    {
        _logger.LogInformation("Processando e-mails pendentes manualmente");

        var command = new ProcessPendingEmailsCommand();
        var result = await _mediator.Send(command);

        return Ok(new { message = $"{result} e-mails adicionados à fila de processamento." });
    }
}
