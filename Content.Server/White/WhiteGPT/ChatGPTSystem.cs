using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.Speech;
using Content.Server.White.Sponsors;
using Content.Shared.Humanoid;
using Content.Shared.Interaction.Events;
using Content.Shared.PAI;
using Content.Shared.Popups;
using Content.Shared.Timing;
using Content.Shared.White;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;

namespace Content.Server.White.WhiteGPT;

public sealed class ChatGPTSystem : EntitySystem
{
    [Dependency] private readonly SponsorsManager _sponsorsManager = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly ChatSystem _chatManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly UseDelaySystem _delay = default!;



    private HttpClient _httpClient = default!;
    private string _apiUrl = default!;

    private int[] allowedTiers = {5, 3, 2, 1};

    public override void Initialize()
    {
        base.Initialize();
        _httpClient = new HttpClient();
        SubscribeLocalEvent<ChatGPTComponent, ListenEvent>(OnEntitySpeak);
        SubscribeLocalEvent<ChatGPTComponent, UseInHandEvent>(OnUseInHand);
        _cfg.OnValueChanged(WhiteCVars.ChatGptApi, s => _apiUrl = s, true);
    }

    private void OnUseInHand(EntityUid uid, ChatGPTComponent component, UseInHandEvent args)
    {
        if(_delay.ActiveDelay(uid)) return;

        _delay.BeginDelay(uid);

        if(!TryComp<MetaDataComponent>(args.User, out var userMetadata)) return;
        if(!TryComp<ActorComponent>(args.User, out var actor)) return;
        if(!TryComp<MetaDataComponent>(component.Owner, out var paiMetadata)) return;
        if (!AllowToInteract(actor))
        {
            _chatManager.TrySendInGameICMessage(uid, "Доступ запрещен. Ваш корпоративный рейтинг слишком низкий.", InGameICChatType.Speak, false);
            return;
        }

        if (!string.IsNullOrEmpty(component.GPTOwner))
        {
            _chatManager.TrySendInGameICMessage(uid, component.GPTOwner == userMetadata.EntityName
                    ? $"{userMetadata.EntityName}, вы уже авторизованы! Чтобы задать мне вопрос, обратитесь ко мне по имени ПАИ"
                    : $"Я уже принадлежу другому пользователю - {component.GPTOwner}", InGameICChatType.Speak, false);

            return;
        }


        component.GPTOwner = userMetadata.EntityName;
        paiMetadata.EntityDescription += $" Данный ПАИ принадлежит {userMetadata.EntityName}";


        _chatManager.TrySendInGameICMessage(uid, $"Пользователь был успешно авторизован! Приветствую вас {component.GPTOwner}", InGameICChatType.Speak, false);
        _appearance.SetData(uid, PAIVisuals.Status, PAIStatus.On);

        Dirty(paiMetadata);
    }

    private async void OnEntitySpeak(EntityUid uid, ChatGPTComponent component, ListenEvent args)
    {
        if(!args.Message.ToLower().StartsWith("пии") || HasComp<ChatGPTComponent>(args.Source)) return;

        await Task.Delay(100);

        if (component.Thinking)
        {
            _chatManager.TrySendInGameICMessage(uid, "Я уже думаю над другим вопросом, подождите!", InGameICChatType.Speak, false);
            return;
        }

        if (string.IsNullOrEmpty(component.GPTOwner))
        {
            _chatManager.TrySendInGameICMessage(uid, "Сначала нужно инициализировать меня", InGameICChatType.Speak, false);
            return;
        }

        var userData = EnsureComp<MetaDataComponent>(args.Source);
        if (userData.EntityName != component.GPTOwner)
        {
            _chatManager.TrySendInGameICMessage(uid, $"Простите, но я не могу ответить вам на вопрос. Я принадлежу другому пользователю - {component.GPTOwner}", InGameICChatType.Speak, false);
            return;
        }

        _chatManager.TrySendInGameICMessage(uid, "Ожидайте ответа", InGameICChatType.Speak, false);

        var gptMessage = await RequestGPT(component, userData.EntityName, args.Message);

        _chatManager.TrySendInGameICMessage(uid, gptMessage, InGameICChatType.Speak, false);
    }

    private async Task<string> RequestGPT(ChatGPTComponent component, string speakerName, string request)
    {
        component.Thinking = true;

        _appearance.SetData(component.Owner, PAIVisuals.Status, PAIStatus.Searching);

        var message = "Сервера НТ перегружены, попробуйте чуть позже!";
        var url = $"paiOwner={speakerName}&request={request}";

        try
        {
            var response = await _httpClient.GetAsync($"{_apiUrl}{url}");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                message = await response.Content.ReadAsStringAsync();

            }
        }
        catch (Exception e)
        {
            // ignored
        }

        component.Thinking = false;

        _appearance.SetData(component.Owner, PAIVisuals.Status, PAIStatus.On);
        return message;
    }

    private bool AllowToInteract(ActorComponent actorComponent)
    {
        return _sponsorsManager.TryGetInfo(actorComponent.PlayerSession.UserId, out var sponsor) && allowedTiers.Any(x => x == sponsor.Tier);
    }
}
