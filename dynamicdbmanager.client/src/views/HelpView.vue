<template>
  <div class="help-page">
    <section class="help-hero glass-panel">
      <div class="hero-icon">?</div>
      <div class="hero-copy">
        <div class="eyebrow">Справка и руководство</div>
        <h2>Как пользоваться Dynamic DB Manager</h2>
        <p>Здесь собраны основные возможности приложения: работа с таблицами, поиск, Excel, колонки, модальные формы, доступ и настройки интерфейса.</p>
      </div>
      <div class="hero-badge">v1</div>
    </section>

    <div class="help-layout">
      <aside class="help-sidebar glass-panel">
        <div class="sidebar-title">Разделы</div>
        <button v-for="item in sections" :key="item.id" type="button" class="sidebar-link" :class="{ active: activeSection === item.id }" @click="scrollTo(item.id)">
          <span class="sidebar-icon">{{ item.icon }}</span>
          <span>{{ item.title }}</span>
        </button>
      </aside>

      <main class="help-content">
        <section id="start" class="help-card glass-panel">
          <div class="section-head"><span class="section-number">01</span><div><h3>С чего начать</h3><p>Главная страница показывает доступные таблицы.</p></div></div>
          <div class="info-grid three">
            <article><b>Таблицы</b><span>Открывайте рабочую область таблицы и переходите между таблицами во вкладках.</span></article>
            <article><b>Категории</b><span>Используйте меню категорий в верхней панели, чтобы быстро отфильтровать список таблиц.</span></article>
            <article><b>Доступ</b><span>Администратор управляет пользователями и правами через отдельный раздел.</span></article>
          </div>
        </section>

        <section id="table" class="help-card glass-panel">
          <div class="section-head"><span class="section-number">02</span><div><h3>Работа с таблицей</h3><p>Открытая таблица — основное рабочее пространство приложения.</p></div></div>
          <div class="step-list">
            <div class="step"><strong>Поиск</strong><span>Введите текст в поле поиска, чтобы отфильтровать записи текущей таблицы.</span></div>
            <div class="step"><strong>Глобальный поиск</strong><span>Переключатель «Глобальный» выполняет поиск по доступным таблицам.</span></div>
            <div class="step"><strong>Добавить</strong><span>Нажмите «Добавить», заполните поля и сохраните новую запись.</span></div>
            <div class="step"><strong>Редактирование</strong><span>Двойной клик по строке открывает редактирование. Также доступны кнопки «Изменить» и «Удалить».</span></div>
          </div>
        </section>

        <section id="columns" class="help-card glass-panel">
          <div class="section-head"><span class="section-number">03</span><div><h3>Колонки как в Excel</h3><p>Ширину и отображение столбцов можно настраивать прямо в таблице.</p></div></div>
          <div class="feature-list">
            <div><b>Растягивание</b><span>Потяните за правую границу заголовка колонки, чтобы изменить ширину.</span></div>
            <div><b>Автоподбор</b><span>Двойной клик по границе колонки подбирает удобную ширину.</span></div>
            <div><b>ПКМ по заголовку</b><span>Открывает контекстное меню дополнительных действий с колонкой.</span></div>
            <div><b>Длинный текст</b><span>При переносе текста содержимое можно просматривать непосредственно в ячейке, без обязательного открытия записи.</span></div>
            <div><b>Настройки</b><span>Ширина и пользовательские параметры представления сохраняются локально для конкретной таблицы.</span></div>
          </div>
        </section>

        <section id="actions" class="help-card glass-panel">
          <div class="section-head"><span class="section-number">04</span><div><h3>Действия со строками</h3><p>Редактирование и удаление доступны непосредственно из строки.</p></div></div>
          <div class="action-demo">
            <div class="demo-action"><span>↕</span><span>Двойной клик по строке — открыть редактирование</span></div>
            <div class="demo-action danger"><span>×</span><span>Крестик справа — удалить запись</span></div>
          </div>
          <p class="tip">Перед удалением проверьте нужную строку. Крестик находится в отдельной узкой колонке справа и не занимает место полноценного блока «Действия».</p>
        </section>

        <section id="modals" class="help-card glass-panel">
          <div class="section-head"><span class="section-number">05</span><div><h3>Модалки и поля</h3><p>Настройки структуры таблицы разделены на две части.</p></div></div>
          <div class="info-grid two">
            <article><b>Колонки таблицы</b><span>Определяют, какие поля показываются непосредственно в основной таблице. Для поля можно выбрать «Текст» или «Флаг».</span></article>
            <article><b>Поля модалки</b><span>Определяют дополнительные поля формы записи, которые используются при добавлении и редактировании.</span></article>
          </div>
          <div class="callout"><b>Совет:</b> сначала настройте структуру колонок и модалки, затем добавляйте записи — так форма сразу будет соответствовать рабочей схеме.</div>
        </section>

        <section id="excel" class="help-card glass-panel">
          <div class="section-head"><span class="section-number">06</span><div><h3>Excel</h3><p>Импорт и экспорт доступны прямо из рабочей области таблицы.</p></div></div>
          <div class="step-list">
            <div class="step"><strong>Экспорт</strong><span>Откройте кнопку «Excel» и используйте экспорт для получения данных из текущей таблицы.</span></div>
            <div class="step"><strong>Импорт</strong><span>Загрузите Excel-файл через окно Excel и выберите нужный режим импорта.</span></div>
            <div class="step"><strong>Проверка</strong><span>После операции обновите представление и убедитесь, что количество записей соответствует ожиданиям.</span></div>
          </div>
        </section>

        <section id="admin" class="help-card glass-panel">
          <div class="section-head"><span class="section-number">07</span><div><h3>Доступ и администрирование</h3><p>Раздел «Доступ» доступен пользователям с правами администратора.</p></div></div>
          <div class="info-grid two">
            <article><b>Пользователи</b><span>Выберите пользователя и просмотрите доступные ему возможности.</span></article>
            <article><b>Разрешения</b><span>Настройте права на работу с функциями и данными приложения в административном интерфейсе.</span></article>
          </div>
        </section>

        <section id="interface" class="help-card glass-panel">
          <div class="section-head"><span class="section-number">08</span><div><h3>Тема и фон</h3><p>Интерфейс поддерживает разные визуальные режимы.</p></div></div>
          <div class="info-grid three">
            <article><b>День</b><span>Светлый интерфейс с тёмным текстом.</span></article>
            <article><b>Ночь</b><span>Тёмный интерфейс с контрастными элементами.</span></article>
            <article><b>Авто</b><span>Режим определяется настройками системы.</span></article>
          </div>
          <p class="tip">Фоновая сцена выбирается отдельно от темы. Если активирован визуальный фон, панели приложения сохраняют собственную читаемую подложку.</p>
        </section>

        <section id="shortcuts" class="help-card glass-panel">
          <div class="section-head"><span class="section-number">09</span><div><h3>Полезные привычки</h3><p>Несколько простых правил для комфортной работы.</p></div></div>
          <div class="check-list">
            <span>✓ Перед удалением проверьте выбранную таблицу.</span>
            <span>✓ Для длинных полей используйте изменение ширины колонки.</span>
            <span>✓ Для структуры данных сначала настройте «Колонки» и «Модалку».</span>
            <span>✓ Для больших наборов данных используйте поиск и пагинацию.</span>
            <span>✓ Административные изменения прав выполняйте осознанно — они влияют на доступ пользователей.</span>
          </div>
        </section>

        <footer class="help-footer glass-panel">Dynamic DB Manager · Справочник пользователя</footer>
      </main>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const activeSection = ref('start')

const sections = [
  { id: 'start', title: 'С чего начать', icon: '⌂' },
  { id: 'table', title: 'Работа с таблицей', icon: '▦' },
  { id: 'columns', title: 'Колонки', icon: '↔' },
  { id: 'actions', title: 'Действия', icon: '✎' },
  { id: 'modals', title: 'Модалки и поля', icon: '□' },
  { id: 'excel', title: 'Excel', icon: '⇩' },
  { id: 'admin', title: 'Доступ', icon: '◉' },
  { id: 'interface', title: 'Тема и фон', icon: '◐' },
  { id: 'shortcuts', title: 'Полезные привычки', icon: '✓' }
]

function scrollTo(id) {
  activeSection.value = id
  document.getElementById(id)?.scrollIntoView({ behavior: 'smooth', block: 'start' })
}
</script>

<style scoped>
.help-page{max-width:1440px;margin:0 auto;padding:12px 0 90px;color:var(--app-text)}
.help-hero{display:flex;align-items:center;gap:18px;padding:22px;border-radius:22px;margin-bottom:14px}
.hero-icon{width:52px;height:52px;display:grid;place-items:center;border-radius:16px;background:var(--app-accent-soft);border:1px solid var(--app-accent-border);color:var(--app-primary);font-size:25px;font-weight:800;flex:none}
.hero-copy{min-width:0}.eyebrow{font-size:11px;letter-spacing:.08em;text-transform:uppercase;color:var(--app-muted);margin-bottom:4px}.hero-copy h2{margin:0;font-size:25px;line-height:1.2}.hero-copy p{margin:8px 0 0;max-width:900px;color:var(--app-muted);font-size:13px}.hero-badge{margin-left:auto;align-self:flex-start;border:1px solid var(--app-border);background:var(--app-control-soft);color:var(--app-muted);padding:6px 10px;border-radius:999px;font-size:11px}
.help-layout{display:grid;grid-template-columns:230px minmax(0,1fr);gap:14px;align-items:start}.help-sidebar{position:sticky;top:92px;padding:10px;border-radius:20px}.sidebar-title{padding:8px 10px 9px;color:var(--app-muted);font-size:11px;text-transform:uppercase;letter-spacing:.07em}.sidebar-link{display:flex;width:100%;align-items:center;gap:10px;min-height:40px;padding:8px 10px;border:1px solid transparent;border-radius:11px;background:transparent;color:var(--app-text-soft);text-align:left;cursor:pointer;transition:.18s}.sidebar-link:hover{background:var(--app-hover);border-color:var(--app-border);color:var(--app-text)}.sidebar-link.active{background:var(--app-accent-soft);border-color:var(--app-accent-border);color:var(--app-text)}.sidebar-icon{width:20px;text-align:center;color:var(--app-primary);font-weight:700}
.help-content{display:grid;gap:12px}.help-card{padding:20px;border-radius:20px;scroll-margin-top:96px}.section-head{display:flex;gap:12px;align-items:flex-start;margin-bottom:16px}.section-number{display:inline-grid;place-items:center;width:34px;height:34px;border-radius:11px;flex:none;background:var(--app-accent-soft);border:1px solid var(--app-accent-border);color:var(--app-primary);font-size:11px;font-weight:800}.section-head h3{margin:1px 0 3px;font-size:18px}.section-head p{margin:0;color:var(--app-muted);font-size:12px}
.info-grid{display:grid;gap:10px}.info-grid.two{grid-template-columns:repeat(2,minmax(0,1fr))}.info-grid.three{grid-template-columns:repeat(3,minmax(0,1fr))}.info-grid article,.feature-list>div,.step,.check-list span{padding:13px;border:1px solid var(--app-border);border-radius:14px;background:var(--app-control-soft)}.info-grid b,.feature-list b{display:block;color:var(--app-text);font-size:13px;margin-bottom:4px}.info-grid span,.feature-list span,.step span{display:block;color:var(--app-muted);font-size:12px;line-height:1.6}
.step-list,.feature-list,.check-list{display:grid;gap:8px}.step{display:grid;grid-template-columns:170px minmax(0,1fr);gap:12px}.step strong{color:var(--app-primary-text);font-size:12px}.feature-list{grid-template-columns:repeat(2,minmax(0,1fr))}.feature-list>div{background:var(--app-control)}.action-demo{display:flex;gap:10px;flex-wrap:wrap}.demo-action{display:inline-flex;align-items:center;gap:8px;min-height:42px;padding:0 13px;border:1px solid var(--app-border);border-radius:12px;background:var(--app-control);color:var(--app-text-soft)}.demo-action span:first-child{color:var(--app-primary);font-weight:800}.demo-action.danger span:first-child{color:var(--app-danger)}.tip{margin:12px 0 0;padding:11px 13px;border-left:3px solid var(--app-primary);background:var(--app-control-soft);border-radius:0 11px 11px 0;color:var(--app-muted);font-size:12px}.callout{margin-top:12px;padding:12px 13px;border:1px solid var(--app-accent-border);border-radius:13px;background:var(--app-accent-soft);color:var(--app-text-soft);font-size:12px}.help-footer{padding:14px 18px;border-radius:16px;color:var(--app-muted);font-size:11px;text-align:center}
@media(max-width:900px){.help-layout{grid-template-columns:1fr}.help-sidebar{position:sticky;top:76px;z-index:8;display:flex;overflow:auto;gap:4px;padding:7px}.sidebar-title{display:none}.sidebar-link{width:auto;flex:none}.info-grid.three,.info-grid.two,.feature-list{grid-template-columns:1fr}.step{grid-template-columns:1fr;gap:4px}}
@media(max-width:600px){.help-page{padding-inline:4px}.help-hero{padding:16px}.hero-icon{width:44px;height:44px;border-radius:14px}.hero-copy h2{font-size:20px}.hero-copy p{font-size:12px}.hero-badge{display:none}.help-card{padding:15px}.section-head h3{font-size:16px}.demo-action{width:100%}}
</style>
