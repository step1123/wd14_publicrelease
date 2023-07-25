game-ticker-restart-round = Перезапуск раунда...
game-ticker-start-round = Раунд начинается...
game-ticker-start-round-cannot-start-game-mode-fallback = Не удалось запустить режим { $failedGameMode }! Запускаем { $fallbackMode }...
game-ticker-start-round-cannot-start-game-mode-restart = Не удалось запустить режим { $failedGameMode }! Перезапуск раунда...
game-ticker-unknown-role = Неизвестный
game-ticker-delay-start = Начало раунда было отложено на { $seconds } секунд.
game-ticker-pause-start = Начало раунда было приостановлено.
game-ticker-pause-start-resumed = Отсчет начала раунда возобновлен.
game-ticker-player-join-game-message = Добро пожаловать на Космическую Станцию 14! Если вы играете впервые, обязательно нажмите ESC на клавиатуре и прочитайте правила игры, а также не бойтесь просить помощи в "Админ помощь".
game-ticker-get-info-text =
    Раунд: [color=white]#{ $roundId }[/color]
    Режим: [color=white]{ $gmTitle }[/color]###Игроки: [color=white]{ $playerCount }[/color]
    Карта: [color=white]{ $mapName }[/color]###- [color=yellow]{ $desc }[/color]
game-ticker-get-info-preround-text =
    Раунд: [color=white]#{ $roundId }[/color]
    Режим: [color=white]{ $gmTitle }[/color]###Игроки: [color=white]{ $playerCount }[/color] ([color=white]{ $readyCount }[/color] { $readyCount ->
        [one] готов
       *[other] готовы
    })
    Карта: [color=white]{ $mapName }[/color]###- [color=yellow]{ $desc }[/color]
game-ticker-no-map-selected = [color=red]Карта ещё не выбрана![/color]
game-ticker-player-no-jobs-available-when-joining = При попытке присоединиться к игре ни одной роли не было доступно.
# Displayed in chat to admins when a player joins
player-join-message = Игрок { $name } присоединился к серверу!
# Displayed in chat to admins when a player leaves
player-leave-message = Игрок { $name } покинул сервер!
latejoin-arrival-announcement =
    { $character } ({ $job }) { $gender ->
        [male] прибыл
        [female] прибыла
        [epicene] прибыли
       *[neuter] прибыл
    } на станцию!
latejoin-arrival-sender = Станции
player-join-message = Игрок { $name } зашёл!
latejoin-arrivals-direction = Вскоре прибудет шаттл, который доставит вас на вашу станцию.
player-first-join-message = Игрок { $name } зашёл на сервер впервые.
player-leave-message = Игрок { $name } вышел!
