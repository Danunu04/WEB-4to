<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="actividades.aspx.cs" Inherits="gymAppV2.Actividades.actividades" MasterPageFile="~/DashBoard.Master" Title="Actividades" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        :root {
            --color-bg: #FFF8F0;
            --color-surface: #FFFFFF;
            --color-surface-2: #FDF6EC;
            --color-text: #2D2D2D;
            --color-muted: #6B6B6B;
            --color-border: #E8E0D5;
            --color-accent-pink: #FFB5C5;
            --color-accent-pink-light: #FFE5EB;
            --color-accent-mint: #B5EAD7;
            --color-accent-mint-light: #E5F9F0;
            --color-accent-lavender: #C7B5FF;
            --color-accent-lavender-light: #F0EBFF;
            --color-accent-peach: #FFD5B5;
            --color-accent-peach-light: #FFF0E5;
            --color-accent-sky: #B5D5FF;
            --color-accent-sky-light: #E5F0FF;
            --radius-sm: 0.375rem;
            --radius-md: 0.5rem;
            --radius-lg: 0.75rem;
            --radius-xl: 1rem;
        }
        .calendar-container { max-width: 100%; margin: 0; padding: 1.5rem; }
        .calendar-header { display: flex; align-items: center; justify-content: space-between; flex-wrap: wrap; gap: 0.75rem; margin-bottom: 1.25rem; }
        .calendar-title h2 { font-family: 'Fraunces', serif; font-size: 1.5rem; font-weight: 700; color: var(--color-text); margin: 0; }
        .calendar-title p { color: var(--color-muted); margin: 0.25rem 0 0 0; font-size: 0.875rem; }
        .calendar-nav { display: flex; align-items: center; gap: 0.5rem; }
        .calendar-nav button { padding: 0.5rem; border-radius: var(--radius-lg); border: 1px solid var(--color-border); background: var(--color-surface); cursor: pointer; }
        .calendar-nav button:hover { background: var(--color-surface-2); }
        .btn-primary { display: flex; align-items: center; gap: 0.5rem; padding: 0.5rem 1rem; background: var(--color-accent-lavender); color: white; border: none; border-radius: var(--radius-lg); cursor: pointer; font-weight: 500; }
        .btn-primary:hover { transform: translateY(-2px); }

        .calendar-grid { background: var(--color-surface); border: 1px solid var(--color-border); border-radius: var(--radius-xl); overflow: hidden; }
        .calendar-weekdays { display: grid; grid-template-columns: repeat(7, 1fr); background: var(--color-surface-2); border-bottom: 1px solid var(--color-border); }
        .calendar-weekdays div { padding: 0.75rem; text-align: center; font-size: 0.875rem; font-weight: 500; color: var(--color-muted); }
        .calendar-days { display: grid; grid-template-columns: repeat(7, 1fr); }
        .calendar-day { min-height: 6rem; padding: 0.5rem; border-bottom: 1px solid var(--color-border); border-right: 1px solid var(--color-border); cursor: pointer; transition: background-color 120ms; }
        .calendar-day:hover { background: var(--color-surface-2); }
        .calendar-day.selected { background: var(--color-accent-lavender-light); }
        .calendar-day.empty { background: var(--color-bg); opacity: 0.5; pointer-events: none; }
        .calendar-day:nth-child(7n) { border-right: none; }
        .day-number { font-size: 0.875rem; font-weight: 500; color: var(--color-muted); margin-bottom: 0.25rem; }
        .class-item { font-size: 0.625rem; padding: 0.125rem 0.375rem; border-radius: var(--radius-sm); background: var(--color-surface); border: 1px solid var(--color-border); margin-bottom: 0.125rem; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
        .class-item.pink { border-left: 2px solid var(--color-accent-pink); }
        .class-item.mint { border-left: 2px solid var(--color-accent-mint); }
        .class-item.lavender { border-left: 2px solid var(--color-accent-lavender); }
        .class-item.peach { border-left: 2px solid var(--color-accent-peach); }
        .class-item.sky { border-left: 2px solid var(--color-accent-sky); }
        .class-name { font-weight: 500; color: var(--color-text); overflow: hidden; text-overflow: ellipsis; }
        .class-time { font-size: 0.625rem; color: var(--color-muted); font-family: 'JetBrains Mono', monospace; }
        .more-classes { font-size: 0.625rem; text-align: center; color: var(--color-muted); }

        .selected-day-panel { margin-top: 1.25rem; background: var(--color-surface); border: 1px solid var(--color-border); border-radius: var(--radius-xl); padding: 1rem; animation: fadeIn 0.3s ease-out; }
        .selected-day-panel h3 { font-family: 'Fraunces', serif; font-size: 1.125rem; font-weight: 700; color: var(--color-text); margin: 0 0 0.75rem 0; }
        .selected-class { display: flex; align-items: center; justify-content: space-between; padding: 0.75rem; border-radius: var(--radius-lg); background: var(--color-surface-2); margin-bottom: 0.5rem; }
        .selected-class:last-child { margin-bottom: 0; }
        .selected-class-info { display: flex; align-items: center; gap: 0.75rem; }
        .class-icon { width: 2.5rem; height: 2.5rem; border-radius: var(--radius-lg); display: flex; align-items: center; justify-content: center; font-family: 'Fraunces', serif; font-weight: 700; color: white; font-size: 0.875rem; }
        .class-icon.pink { background: var(--color-accent-pink); }
        .class-icon.mint { background: var(--color-accent-mint); }
        .class-icon.lavender { background: var(--color-accent-lavender); }
        .class-icon.peach { background: var(--color-accent-peach); }
        .class-icon.sky { background: var(--color-accent-sky); }
        .class-details h4 { font-weight: 500; color: var(--color-text); margin: 0; }
        .class-details p { font-size: 0.875rem; color: var(--color-muted); font-family: 'JetBrains Mono', monospace; margin: 0.125rem 0 0 0; }
        .btn-details { font-size: 0.875rem; font-weight: 500; color: var(--color-accent-lavender); background: none; border: none; cursor: pointer; text-decoration: underline; }

        .modal-overlay { position: fixed; inset: 0; background: rgba(0, 0, 0, 0.5); display: flex; align-items: center; justify-content: center; z-index: 1000; padding: 1rem; }
        .modal-content { background: var(--color-surface); border: 1px solid var(--color-border); border-radius: var(--radius-xl); padding: 1.5rem; width: 100%; max-width: 28rem; animation: fadeIn 0.3s ease-out; }
        .modal-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 1rem; }
        .modal-title { font-family: 'Fraunces', serif; font-size: 1.25rem; font-weight: 700; color: var(--color-text); margin: 0; }
        .modal-close { padding: 0.5rem; border-radius: var(--radius-lg); background: none; border: none; cursor: pointer; }
        .modal-close:hover { background: var(--color-surface-2); }
        .form-group { margin-bottom: 1rem; }
        .form-group label { display: block; font-size: 0.875rem; font-weight: 500; color: var(--color-text); margin-bottom: 0.25rem; }
        .form-group input, .form-group select { width: 100%; padding: 0.5rem 0.75rem; border-radius: var(--radius-lg); border: 1px solid var(--color-border); font-family: 'DM Sans', sans-serif; }
        .form-group input:focus, .form-group select:focus { outline: none; box-shadow: 0 0 0 2px var(--color-accent-lavender); }
        .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 0.75rem; }
        .modal-actions { display: flex; gap: 0.5rem; padding-top: 1rem; border-top: 1px solid var(--color-border); }
        .btn-secondary { flex: 1; padding: 0.5rem 1rem; border-radius: var(--radius-lg); border: 1px solid var(--color-border); background: var(--color-surface); color: var(--color-muted); cursor: pointer; font-weight: 500; }
        .btn-secondary:hover { background: var(--color-surface-2); }
        .btn-submit { flex: 1; padding: 0.5rem 1rem; border-radius: var(--radius-lg); border: none; background: var(--color-accent-lavender); color: white; cursor: pointer; font-weight: 500; }
        .btn-submit:hover { transform: translateY(-2px); }

        @keyframes fadeIn { from { opacity: 0; transform: translateY(8px); } to { opacity: 1; transform: translateY(0); } }
        .animate-fade-in { animation: fadeIn 0.3s ease-out; }

        /* Highlight active nav link */
        .sidebar-menu li a[href*="Actividades"] {
            background: var(--color-accent-lavender);
            color: white;
        }
        .sidebar-menu li a[href*="Actividades"] i,
        .sidebar-menu li a[href*="Actividades"] .menu-text {
            color: white;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="calendar-container">
        <div class="calendar-header animate-fade-in">
            <div class="calendar-title">
                <h2>Calendario de Actividades</h2>
                <p id="currentMonthDisplay"></p>
            </div>
            <div class="calendar-nav">
                <button type="button" id="prevMonth" class="btn-transition">
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <polyline points="15 18 9 12 15 6"/>
                    </svg>
                </button>
                <button type="button" id="nextMonth" class="btn-transition">
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <polyline points="9 18 15 12 9 6"/>
                    </svg>
                </button>
                <button type="button" id="btnNewActivity" class="btn-primary btn-transition">
                    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <line x1="12" y1="5" x2="12" y2="19"/>
                        <line x1="5" y1="12" x2="19" y2="12"/>
                    </svg>
                    <span>Nueva Actividad</span>
                </button>
            </div>
        </div>

        <div class="calendar-grid animate-fade-in">
            <div class="calendar-weekdays">
                <div>Dom</div>
                <div>Lun</div>
                <div>Mar</div>
                <div>Mié</div>
                <div>Jue</div>
                <div>Vie</div>
                <div>Sáb</div>
            </div>
            <div class="calendar-days" id="calendarDays"></div>
        </div>

        <div id="selectedDayPanel" class="selected-day-panel" style="display: none;"></div>
    </div>

    <!-- Modal Nueva Actividad -->
    <div id="activityModal" class="modal-overlay" style="display: none;">
        <div class="modal-content" onclick="event.stopPropagation();">
            <div class="modal-header">
                <h3 class="modal-title">Nueva Actividad</h3>
                <button type="button" class="modal-close" id="closeModal">
                    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <line x1="18" y1="6" x2="6" y2="18"/>
                        <line x1="6" y1="6" x2="18" y2="18"/>
                    </svg>
                </button>
            </div>
            <div class="form-group">
                <label>Nombre de la actividad</label>
                <input type="text" id="activityName" placeholder="Ej: Yoga Flow" />
            </div>
            <div class="form-group">
                <label>Instructor</label>
                <input type="text" id="instructorName" placeholder="Ej: Ana García" />
            </div>
            <div class="form-row">
                <div class="form-group">
                    <label>Horario</label>
                    <input type="time" id="activityTime" />
                </div>
                <div class="form-group">
                    <label>Duración (min)</label>
                    <input type="number" id="activityDuration" placeholder="45" />
                </div>
            </div>
            <div class="form-group">
                <label>Color</label>
                <select id="activityColor">
                    <option value="pink">Rosa</option>
                    <option value="mint">Menta</option>
                    <option value="lavender">Lavanda</option>
                    <option value="peach">Durazno</option>
                    <option value="sky">Celeste</option>
                </select>
            </div>
            <div class="modal-actions">
                <button type="button" class="btn-secondary" id="cancelActivity">Cancelar</button>
                <button type="button" class="btn-submit" id="saveActivity">Crear Actividad</button>
            </div>
        </div>
    </div>

    <script>
        // Datos de actividades por día (simulados)
        const activitiesByDay = {
            1: [{ name: 'Yoga Flow', time: '08:00', color: 'pink', instructor: 'Ana García' }, { name: 'CrossFit', time: '18:00', color: 'lavender', instructor: 'Carlos Ruiz' }],
            3: [{ name: 'Pilates', time: '10:00', color: 'mint', instructor: 'María López' }],
            5: [{ name: 'HIIT', time: '07:00', color: 'peach', instructor: 'David Chen' }, { name: 'Zumba', time: '19:00', color: 'sky', instructor: 'Sofía Martín' }],
            7: [{ name: 'Yoga Flow', time: '09:00', color: 'pink', instructor: 'Ana García' }],
            8: [{ name: 'Spinning', time: '17:00', color: 'mint', instructor: 'María López' }],
            10: [{ name: 'CrossFit', time: '06:00', color: 'lavender', instructor: 'Carlos Ruiz' }, { name: 'Body Pump', time: '20:00', color: 'peach', instructor: 'David Chen' }],
            12: [{ name: 'Pilates', time: '11:00', color: 'mint', instructor: 'María López' }],
            14: [{ name: 'HIIT', time: '07:00', color: 'peach', instructor: 'David Chen' }],
            15: [{ name: 'Yoga Flow', time: '08:00', color: 'pink', instructor: 'Ana García' }, { name: 'Zumba', time: '18:00', color: 'sky', instructor: 'Sofía Martín' }],
            17: [{ name: 'Spinning', time: '17:00', color: 'mint', instructor: 'María López' }],
            19: [{ name: 'CrossFit', time: '06:00', color: 'lavender', instructor: 'Carlos Ruiz' }],
            21: [{ name: 'Body Pump', time: '19:00', color: 'peach', instructor: 'David Chen' }],
            22: [{ name: 'Pilates', time: '10:00', color: 'mint', instructor: 'María López' }],
            24: [{ name: 'HIIT', time: '07:00', color: 'peach', instructor: 'David Chen' }, { name: 'Yoga Flow', time: '20:00', color: 'pink', instructor: 'Ana García' }],
            26: [{ name: 'Zumba', time: '18:00', color: 'sky', instructor: 'Sofía Martín' }],
            28: [{ name: 'Spinning', time: '17:00', color: 'mint', instructor: 'María López' }],
            29: [{ name: 'CrossFit', time: '06:00', color: 'lavender', instructor: 'Carlos Ruiz' }],
            31: [{ name: 'Body Pump', time: '19:00', color: 'peach', instructor: 'David Chen' }],
        };

        const monthNames = ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'];

        let currentDate = new Date();
        let selectedDay = null;

        function renderCalendar() {
            const year = currentDate.getFullYear();
            const month = currentDate.getMonth();

            // Actualizar display del mes
            document.getElementById('currentMonthDisplay').textContent = `${monthNames[month]} ${year}`;

            // Calcular días del mes
            const firstDay = new Date(year, month, 1);
            const lastDay = new Date(year, month + 1, 0);
            const daysInMonth = lastDay.getDate();
            const startingDay = firstDay.getDay();

            const calendarDays = document.getElementById('calendarDays');
            calendarDays.innerHTML = '';

            // Días vacíos antes del primer día del mes
            for (let i = 0; i < startingDay; i++) {
                const emptyDay = document.createElement('div');
                emptyDay.className = 'calendar-day empty';
                calendarDays.appendChild(emptyDay);
            }

            // Días del mes
            for (let day = 1; day <= daysInMonth; day++) {
                const dayElement = document.createElement('div');
                dayElement.className = 'calendar-day';
                if (selectedDay === day) {
                    dayElement.classList.add('selected');
                }

                const dayNumber = document.createElement('div');
                dayNumber.className = 'day-number';
                dayNumber.textContent = day;
                dayElement.appendChild(dayNumber);

                const activities = activitiesByDay[day] || [];
                activities.slice(0, 3).forEach(activity => {
                    const classItem = document.createElement('div');
                    classItem.className = `class-item ${activity.color}`;
                    classItem.innerHTML = `
                        <div class="class-name">${activity.name}</div>
                        <div class="class-time">${activity.time}</div>
                    `;
                    dayElement.appendChild(classItem);
                });

                if (activities.length > 3) {
                    const moreClasses = document.createElement('div');
                    moreClasses.className = 'more-classes';
                    moreClasses.textContent = `+${activities.length - 3} más`;
                    dayElement.appendChild(moreClasses);
                }

                dayElement.addEventListener('click', () => selectDay(day));
                calendarDays.appendChild(dayElement);
            }
        }

        function selectDay(day) {
            selectedDay = day;
            renderCalendar();
            showSelectedDayPanel(day);
        }

        function showSelectedDayPanel(day) {
            const panel = document.getElementById('selectedDayPanel');
            const year = currentDate.getFullYear();
            const month = currentDate.getMonth();

            const activities = activitiesByDay[day] || [];

            if (activities.length === 0) {
                panel.style.display = 'none';
                return;
            }

            panel.innerHTML = `
                <h3>Actividades del ${day} de ${monthNames[month]}</h3>
                ${activities.map(activity => `
                    <div class="selected-class">
                        <div class="selected-class-info">
                            <div class="class-icon ${activity.color}">${activity.name.charAt(0)}</div>
                            <div class="class-details">
                                <h4>${activity.name}</h4>
                                <p>${activity.time} - ${activity.instructor}</p>
                            </div>
                        </div>
                        <button class="btn-details">Ver detalles</button>
                    </div>
                `).join('')}
            `;
            panel.style.display = 'block';
        }

        // Event Listeners para navegación
        document.getElementById('prevMonth').addEventListener('click', () => {
            currentDate.setMonth(currentDate.getMonth() - 1);
            selectedDay = null;
            document.getElementById('selectedDayPanel').style.display = 'none';
            renderCalendar();
        });

        document.getElementById('nextMonth').addEventListener('click', () => {
            currentDate.setMonth(currentDate.getMonth() + 1);
            selectedDay = null;
            document.getElementById('selectedDayPanel').style.display = 'none';
            renderCalendar();
        });

        // Modal functionality
        const modal = document.getElementById('activityModal');
        const btnNewActivity = document.getElementById('btnNewActivity');
        const closeModal = document.getElementById('closeModal');
        const cancelActivity = document.getElementById('cancelActivity');
        const saveActivity = document.getElementById('saveActivity');

        btnNewActivity.addEventListener('click', () => {
            modal.style.display = 'flex';
        });

        closeModal.addEventListener('click', () => {
            modal.style.display = 'none';
        });

        cancelActivity.addEventListener('click', () => {
            modal.style.display = 'none';
        });

        modal.addEventListener('click', () => {
            modal.style.display = 'none';
        });

        saveActivity.addEventListener('click', () => {
            const name = document.getElementById('activityName').value;
            const instructor = document.getElementById('instructorName').value;
            const time = document.getElementById('activityTime').value;
            const duration = document.getElementById('activityDuration').value;
            const color = document.getElementById('activityColor').value;

            if (name && selectedDay) {
                if (!activitiesByDay[selectedDay]) {
                    activitiesByDay[selectedDay] = [];
                }
                activitiesByDay[selectedDay].push({
                    name: name,
                    time: time || '00:00',
                    color: color,
                    instructor: instructor || 'Sin instructor'
                });
                renderCalendar();
                showSelectedDayPanel(selectedDay);
                modal.style.display = 'none';

                // Limpiar formulario
                document.getElementById('activityName').value = '';
                document.getElementById('instructorName').value = '';
                document.getElementById('activityTime').value = '';
                document.getElementById('activityDuration').value = '';
            } else if (!selectedDay) {
                alert('Por favor selecciona un día del calendario');
            }
        });

        // Inicializar calendario
        renderCalendar();
    </script>
</asp:Content>
