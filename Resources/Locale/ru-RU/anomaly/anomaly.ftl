anomaly-component-contact-damage = Аномалия сдирает с вас кожу!
anomaly-vessel-component-anomaly-assigned = Аномалия присвоена сосуду.
anomaly-vessel-component-not-assigned = Этому сосуду не присвоена ни одна аномалия. Попробуйте использовать на нём сканер.
anomaly-vessel-component-assigned = Этому сосуду уже присвоена аномалия.
anomaly-vessel-component-upgrade-output = генерация очков
anomaly-particles-delta = Дельта-частицы
anomaly-particles-epsilon = Эпсилон-частицы
anomaly-particles-zeta = Зета-частицы
anomaly-scanner-component-scan-complete = Сканирование завершено!
anomaly-scanner-ui-title = сканер аномалий
anomaly-scanner-no-anomaly = Нет просканированной аномалии.
anomaly-scanner-severity-percentage = Текущая опасность: [color=gray]{ $percent }[/color]
anomaly-scanner-stability-low = Текущее состояние аномалии: [color=gold]Распад[/color]
anomaly-scanner-stability-medium = Текущее состояние аномалии: [color=forestgreen]Стабильное[/color]
anomaly-scanner-stability-high = Текущее состояние аномалии: [color=crimson]Рост[/color]
anomaly-scanner-point-output = Пассивная генерация очков: [color=gray]{ $point }[/color]
anomaly-scanner-particle-readout = Анализ реакции на частицы:
anomaly-scanner-particle-danger = - [color=crimson]Опасный тип:[/color] { $type }
anomaly-scanner-particle-unstable = - [color=plum]Нестабильный тип:[/color] { $type }
anomaly-scanner-particle-containment = - [color=goldenrod]Сдерживающий тип:[/color] { $type }
anomaly-scanner-pulse-timer = Время до следующего импульса: [color=gray]{ $time }[/color]
anomaly-generator-ui-title = генератор аномалий
anomaly-generator-fuel-display = Топливо:
anomaly-generator-cooldown = Перезарядка: [color=gray]{ $time }[/color]
anomaly-generator-no-cooldown = Перезарядка: [color=gray]Завершена[/color]
anomaly-generator-yes-fire = Статус: [color=forestgreen]Готов[/color]
anomaly-generator-no-fire = Статус: [color=crimson]Не готов[/color]
anomaly-generator-generate = Создать аномалию
anomaly-generator-charges =
    { $charges ->
        [one] { $charges } заряд
       *[other] { $charges } заряды
    }
anomaly-generator-announcement = Была сгенерирована аномалия!
anomaly-command-pulse = Пульсирует аномалию
anomaly-command-supercritical = Доводит аномалию до суперкритического состояния
# Flavor text on the footer
anomaly-generator-flavor-left = Аномалия может возникнуть внутри пользователя.
anomaly-generator-flavor-right = v1.1
ent-AnomalyScanner = сканер аномалий
    .desc = Ручной сканер, созданный для сбора информации о различных аномальных объектах.
    .suffix = { "" }
ent-BaseAnomaly = аномалия
    .desc = Невозможный объект в космосе. Стоит ли вам стоять так близко к нему?
    .suffix = { "" }
ent-AnomalyPyroclastic = аномалия
    .suffix = Пирокластическая
    .desc = Невозможный объект в космосе. Стоит ли вам стоять так близко к нему?
ent-AnomalyGravity = аномалия
    .suffix = Гравитационная
    .desc = Невозможный объект в космосе. Стоит ли вам стоять так близко к нему?
ent-AnomalyElectricity = аномалия
    .suffix = Электрическая
    .desc = Невозможный объект в космосе. Стоит ли вам стоять так близко к нему?
ent-AnomalyFlesh = аномалия
    .suffix = Плоть
    .desc = Невозможный объект в космосе. Стоит ли вам стоять так близко к нему?
ent-AnomalyBluespace = аномалия
    .suffix = Блюспейс
    .desc = Невозможный объект в космосе. Стоит ли вам стоять так близко к нему?
ent-MobFleshLover = уродливая плоть
    .suffix = ИИ
    .desc = Неуклюжая масса плоти, оживленная аномальной энергией.
ent-MobFleshGolem = { ent-MobFleshLover }
    .suffix = ИИ
    .desc = { ent-MobFleshLover.desc }
ent-MobFleshClamp = { ent-MobFleshLover }
    .suffix = ИИ
    .desc = { ent-MobFleshLover.desc }
ent-MobFleshJared = { ent-MobFleshLover }
    .suffix = ИИ
    .desc = { ent-MobFleshLover.desc }
ent-FleshKudzu = сухожилия
    .suffix = { "" }
    .desc = Быстрорастущее скопление мясных сухожилий. ПОЧЕМУ ТЫ НЕ ПРЕКРАЩАЕШЬ НА ЭТО СМОТРЕТЬ?
ent-FleshBlocker = сгусток плоти
    .suffix = { "" }
    .desc = Раздражающий сгусток плоти.
ent-SignAnomaly2 = знак лаборатории аномалий
    .suffix = { "" }
    .desc = Знак, обозначающий лабораторию изучения аномалий.
ent-AnomalyVesselCircuitboard = аномальный сосуд (машинная плата)
    .suffix = { "Машинная плата" }
    .desc = Напечатанная машинная плата для аномального сосуда.
ent-BoozeDispenserMachineCircuitboard = раздатчик алкоголя (машинная плата)
    .suffix = { "Машинная плата" }
    .desc = Напечатанная машинная плата для раздатчика алкоголя.
ent-CrewMonitoringServerMachineCircuitboard = сервер отслеживания экипажа (машинная плата)
    .suffix = { "Машинная плата" }
    .desc = Напечатанная машинная плата для сервера отслеживани экипажа.
ent-SodaDispenserMachineCircuitboard = раздатчик газировки (машинная плата)
    .suffix = { "Машинная плата" }
    .desc = Напечатанная машинная плата для раздатчика газировки.
ent-TelecomServerCircuitboard = сервер связи (машинная плата)
    .suffix = { "Машинная плата" }
    .desc = Напечатанная машинная плата для сервера связи.
ent-PaperArtifactAnalyzer = отчёт анализа артефакта
    .suffix = { "" }
    .desc = Отчёт из оборудовния, забытого временем.
ent-CrewMonitoringServer = сервер мониторинга экипажа
    .desc = Получает и ретранслирует состояние всех активных датчиков скафандра на станции.
    .suffix = { "" }
ent-TelecomServer = сервер телекоммуникации
    .desc = При включении и заполнении ключами шифрования он позволяет осуществлять связь через радиогарнитуру.
    .suffix = { "" }
ent-TelecomServerFilled = { ent-TelecomServer }
    .desc = { ent-TelecomServer.desc }
    .suffix = Заполненный
