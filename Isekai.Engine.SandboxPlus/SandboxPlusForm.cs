using System.ComponentModel;
using System.Globalization;
using Isekai.Engine.Core;
using Isekai.Engine.Modules.Body;
using Isekai.Engine.Modules.Composition;
using Isekai.Engine.Modules.Injuries;
using Isekai.Engine.Modules.Materials;
using Isekai.Engine.Modules.Movement;
using Isekai.Engine.Modules.Needs;
using Isekai.Engine.Modules.Perception;
using Isekai.Engine.Modules.Temperature;
using Isekai.Engine.Modules.Vitals;
using Isekai.Engine.Sandbox.Components;
using Isekai.Engine.Sandbox.Scenarios;

namespace Isekai.Engine.SandboxPlus;

/// <summary>
/// Graphical viewer for sandbox scenarios.
/// </summary>
internal sealed class SandboxPlusForm : Form
{
    private readonly ComboBox _scenarioSelector = new();
    private readonly Button _resetButton = new();
    private readonly Button _playButton = new();
    private readonly Button _stepButton = new();
    private readonly NumericUpDown _speedSelector = new();
    private readonly Label _titleLabel = new();
    private readonly Label _metricsLabel = new();
    private readonly Label _descriptionLabel = new();
    private readonly WorldMapControl _mapControl = new();
    private readonly BodyDiagramControl _bodyDiagram = new();
    private readonly DataGridView _entityGrid = new();
    private readonly System.Windows.Forms.Timer _timer = new();
    private readonly BindingList<EntityViewRow> _rows = [];

    private SandboxSession? _session;
    private EntityId? _selectedEntityId;
    private bool _isPlaying;

    public SandboxPlusForm()
    {
        Text = "Sandbox++ - World Engine";
        MinimumSize = new Size(1180, 720);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Palette.Background;
        ForeColor = Palette.Text;
        Font = new Font("Segoe UI", 10, FontStyle.Regular, GraphicsUnit.Point);

        BuildUi();
        ConfigureTimer();
        LoadScenarios();
    }

    private void BuildUi()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(22),
            BackColor = Palette.Background
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 62));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 38));

        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildVisualizationArea(), 0, 1);
        root.Controls.Add(BuildEntityGrid(), 0, 2);

        Controls.Add(root);
    }

    private Control BuildHeader()
    {
        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = Palette.Background
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        header.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        header.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

        _titleLabel.Text = "Sandbox++";
        _titleLabel.Font = new Font("Segoe UI", 20, FontStyle.Bold, GraphicsUnit.Point);
        _titleLabel.ForeColor = Palette.Text;
        _titleLabel.Dock = DockStyle.Fill;

        _descriptionLabel.ForeColor = Palette.Muted;
        _descriptionLabel.Dock = DockStyle.Fill;
        _descriptionLabel.AutoEllipsis = true;

        _metricsLabel.TextAlign = ContentAlignment.MiddleRight;
        _metricsLabel.ForeColor = Palette.Muted;
        _metricsLabel.Dock = DockStyle.Fill;

        var controls = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            BackColor = Palette.Background,
            WrapContents = false
        };

        ConfigureComboBox(_scenarioSelector);
        ConfigureButton(_resetButton, "Reset");
        ConfigureButton(_playButton, "Play");
        ConfigureButton(_stepButton, "Tick");
        ConfigureSpeedSelector();

        _scenarioSelector.SelectedIndexChanged += (_, _) => ResetScenario();
        _resetButton.Click += (_, _) => ResetScenario();
        _playButton.Click += (_, _) => TogglePlay();
        _stepButton.Click += (_, _) => Step();

        controls.Controls.Add(_resetButton);
        controls.Controls.Add(_playButton);
        controls.Controls.Add(_stepButton);
        controls.Controls.Add(_speedSelector);
        controls.Controls.Add(_scenarioSelector);

        header.Controls.Add(_titleLabel, 0, 0);
        header.Controls.Add(controls, 1, 0);
        header.Controls.Add(_descriptionLabel, 0, 1);
        header.Controls.Add(_metricsLabel, 1, 1);

        return header;
    }

    private Control BuildVisualizationArea()
    {
        var area = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Palette.Background,
            Padding = new Padding(0, 10, 0, 12)
        };
        area.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72));
        area.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));

        _mapControl.Dock = DockStyle.Fill;
        _bodyDiagram.Dock = DockStyle.Fill;

        area.Controls.Add(WrapPanel(_mapControl, "Carte"), 0, 0);
        area.Controls.Add(WrapPanel(_bodyDiagram, "Corps selectionne"), 1, 0);
        return area;
    }

    private Control BuildEntityGrid()
    {
        _entityGrid.Dock = DockStyle.Fill;
        _entityGrid.AutoGenerateColumns = false;
        _entityGrid.DataSource = _rows;
        _entityGrid.BackgroundColor = Palette.Panel;
        _entityGrid.BorderStyle = BorderStyle.None;
        _entityGrid.GridColor = Color.FromArgb(42, 52, 60);
        _entityGrid.EnableHeadersVisualStyles = false;
        _entityGrid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        _entityGrid.ColumnHeadersDefaultCellStyle.BackColor = Palette.PanelAlt;
        _entityGrid.ColumnHeadersDefaultCellStyle.ForeColor = Palette.Muted;
        _entityGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold, GraphicsUnit.Point);
        _entityGrid.DefaultCellStyle.BackColor = Palette.Panel;
        _entityGrid.DefaultCellStyle.ForeColor = Palette.Text;
        _entityGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(51, 78, 96);
        _entityGrid.DefaultCellStyle.SelectionForeColor = Palette.Text;
        _entityGrid.RowHeadersVisible = false;
        _entityGrid.ReadOnly = true;
        _entityGrid.AllowUserToAddRows = false;
        _entityGrid.AllowUserToDeleteRows = false;
        _entityGrid.AllowUserToResizeRows = false;
        _entityGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _entityGrid.RowTemplate.Height = 30;
        _entityGrid.SelectionChanged += (_, _) => UpdateSelectedEntityFromGrid();

        AddColumn("Name", "Entite", 190);
        AddColumn("Position", "Position", 90);
        AddColumn("Needs", "Besoins", 170);
        AddColumn("Temperature", "Temperature", 120);
        AddColumn("State", "Etat", 260);
        AddColumn("Action", "Action", 220);

        return _entityGrid;
    }

    private static Control WrapPanel(Control content, string title)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Palette.Panel,
            Padding = new Padding(1)
        };

        var titleLabel = new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Height = 34,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(14, 0, 0, 0),
            BackColor = Palette.PanelAlt,
            ForeColor = Palette.Muted,
            Font = new Font("Segoe UI", 9, FontStyle.Bold, GraphicsUnit.Point)
        };

        content.Dock = DockStyle.Fill;
        panel.Controls.Add(content);
        panel.Controls.Add(titleLabel);
        return panel;
    }

    private void AddColumn(string propertyName, string headerText, int width)
    {
        _entityGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = propertyName,
            HeaderText = headerText,
            Width = width,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
    }

    private void ConfigureTimer()
    {
        _timer.Interval = 250;
        _timer.Tick += (_, _) => Step();
    }

    private void LoadScenarios()
    {
        _scenarioSelector.DataSource = SandboxScenarioRegistry.All.ToArray();
        _scenarioSelector.DisplayMember = nameof(SandboxScenario.Name);
        _scenarioSelector.ValueMember = nameof(SandboxScenario.Id);

        var movement = SandboxScenarioRegistry.FindById("movement-perception");
        _scenarioSelector.SelectedItem = movement ?? SandboxScenarioRegistry.All.First();
        ResetScenario();
    }

    private void ResetScenario()
    {
        if (_scenarioSelector.SelectedItem is not SandboxScenario scenario)
        {
            return;
        }

        _session = SandboxRuntime.CreateSession(scenario);
        _selectedEntityId = null;
        _isPlaying = false;
        _timer.Stop();
        _playButton.Text = "Play";
        RefreshView();
    }

    private void TogglePlay()
    {
        _isPlaying = !_isPlaying;
        _playButton.Text = _isPlaying ? "Pause" : "Play";

        if (_isPlaying)
        {
            _timer.Start();
        }
        else
        {
            _timer.Stop();
        }
    }

    private void Step()
    {
        if (_session is null)
        {
            return;
        }

        _session.World.Tick(TimeSpan.FromSeconds((double)_speedSelector.Value));
        RefreshView();
    }

    private void RefreshView()
    {
        if (_session is null)
        {
            return;
        }

        EnsureSelection();
        _mapControl.SetSession(_session, _selectedEntityId);
        _bodyDiagram.SetEntity(_session.World, FindSelectedEntity());
        _titleLabel.Text = $"Sandbox++ - {_session.Scenario.Name}";
        _descriptionLabel.Text = _session.Scenario.Description;

        var stats = _session.Map.GetTemperatureStats(_session.World.Time.TickCount);
        _metricsLabel.Text =
            $"Tick {_session.World.Time.TickCount}   Temp {stats.Average:0.0} C   Entites {_session.World.Entities.Count}";

        _rows.Clear();
        foreach (var entity in _session.World.Entities
                     .Where(IsVisibleEntity)
                     .OrderBy(ReadEntityName, StringComparer.Ordinal))
        {
            _rows.Add(CreateRow(entity));
        }

        SelectGridRow(_selectedEntityId);
        _bodyDiagram.SetEntity(_session.World, FindSelectedEntity());
    }

    private EntityViewRow CreateRow(Entity entity)
    {
        return new EntityViewRow(
            entity.Id,
            ReadEntityName(entity),
            FormatPosition(entity),
            FormatNeeds(entity),
            FormatTemperature(entity),
            FormatState(entity),
            FormatAction(entity));
    }

    private void UpdateSelectedEntityFromGrid()
    {
        if (_entityGrid.CurrentRow?.DataBoundItem is not EntityViewRow row)
        {
            return;
        }

        _selectedEntityId = row.EntityId;
        _mapControl.SetSession(_session, _selectedEntityId);
        _bodyDiagram.SetEntity(_session?.World, FindSelectedEntity());
    }

    private void EnsureSelection()
    {
        if (_session is null)
        {
            _selectedEntityId = null;
            return;
        }

        if (_selectedEntityId is not null &&
            _session.World.TryGetEntity(_selectedEntityId.Value, out var selected) &&
            selected is not null)
        {
            return;
        }

        _selectedEntityId = _session.World.Entities
            .Where(IsVisibleEntity)
            .OrderByDescending(entity => entity.HasComponent<BodyStateComponent>())
            .ThenBy(ReadEntityName, StringComparer.Ordinal)
            .FirstOrDefault()
            ?.Id;
    }

    private Entity? FindSelectedEntity()
    {
        if (_session is null || _selectedEntityId is null)
        {
            return null;
        }

        return _session.World.TryGetEntity(_selectedEntityId.Value, out var entity)
            ? entity
            : null;
    }

    private void SelectGridRow(EntityId? entityId)
    {
        if (entityId is null)
        {
            return;
        }

        foreach (DataGridViewRow row in _entityGrid.Rows)
        {
            if (row.DataBoundItem is EntityViewRow viewRow && viewRow.EntityId == entityId)
            {
                row.Selected = true;
                _entityGrid.CurrentCell = row.Cells[0];
                break;
            }
        }
    }

    private static bool IsVisibleEntity(Entity entity)
    {
        return entity.HasComponent<Position2DComponent>() ||
               entity.HasComponent<MovementResultComponent>() ||
               entity.HasComponent<ConsumableResourceComponent>() ||
               entity.HasComponent<WaterSourceComponent>() ||
               entity.HasComponent<BodyIntegrityComponent>();
    }

    private static string ReadEntityName(Entity entity)
    {
        return entity.TryGetComponent<NameComponent>(out var name) && name is not null
            ? name.Name
            : entity.Id.ToString();
    }

    private static string FormatPosition(Entity entity)
    {
        return entity.TryGetComponent<Position2DComponent>(out var position) && position is not null
            ? $"{position.X}, {position.Y}"
            : "-";
    }

    private static string FormatNeeds(Entity entity)
    {
        var parts = new List<string>();

        if (entity.TryGetComponent<EnergyNeedComponent>(out var energy) && energy is not null)
        {
            parts.Add($"E {FormatPercent(energy.CurrentEnergy, energy.MaximumEnergy)}");
        }

        if (entity.TryGetComponent<HydrationNeedComponent>(out var hydration) && hydration is not null)
        {
            parts.Add($"H {FormatPercent(hydration.CurrentHydration, hydration.MaximumHydration)}");
        }

        if (entity.TryGetComponent<ConsumableResourceComponent>(out var food) && food is not null)
        {
            parts.Add($"Food {food.CurrentQuantity:0.#}/{food.MaximumQuantity:0.#}");
        }

        if (entity.TryGetComponent<WaterSourceComponent>(out var water) && water is not null)
        {
            parts.Add($"Water {water.CurrentVolumeLiters:0.#}L");
        }

        return parts.Count == 0 ? "-" : string.Join("   ", parts);
    }

    private static string FormatTemperature(Entity entity)
    {
        return entity.TryGetComponent<TemperatureComponent>(out var temperature) && temperature is not null
            ? $"{temperature.Celsius:0.0} C"
            : "-";
    }

    private static string FormatState(Entity entity)
    {
        var parts = new List<string>();

        if (entity.TryGetComponent<MaterialMassComponent>(out var mass) && mass is not null)
        {
            parts.Add($"{mass.Kilograms:0.#} kg");
        }

        if (entity.TryGetComponent<CompositeMassComponent>(out var compositeMass) && compositeMass is not null)
        {
            parts.Add($"{compositeMass.Kilograms:0.#} kg composite");
        }

        if (entity.TryGetComponent<BodyIntegrityComponent>(out var body) && body is not null)
        {
            parts.Add($"Body {(body.NormalizedIntegrity * 100):0}%");
        }

        if (entity.TryGetComponent<BloodComponent>(out var blood) && blood is not null)
        {
            parts.Add($"Blood {(blood.Ratio * 100):0}%");
        }

        if (entity.TryGetComponent<VitalStateComponent>(out var vital) && vital is not null && !vital.IsAlive)
        {
            parts.Add($"Dead: {vital.DeathReason}");
        }

        if (entity.TryGetComponent<InjuryComponent>(out var injuries) && injuries is not null && injuries.Injuries.Count > 0)
        {
            parts.Add($"Injuries {injuries.Injuries.Count}");
        }

        return parts.Count == 0 ? "-" : string.Join("   ", parts);
    }

    private static string FormatAction(Entity entity)
    {
        var parts = new List<string>();

        if (entity.TryGetComponent<MovementResultComponent>(out var movement) && movement is not null)
        {
            parts.Add($"Move {movement.Result.Outcome} {movement.Result.ActualDistanceMeters:0.##}m");
        }

        if (entity.TryGetComponent<PerceptionCapabilityComponent>(out var perception) && perception is not null)
        {
            parts.Add($"View {perception.MaximumRangeMeters:0.#}m");
        }

        return parts.Count == 0 ? "-" : string.Join("   ", parts);
    }

    private static string FormatPercent(double current, double maximum)
    {
        return maximum <= 0 ? "n/a" : $"{Math.Clamp(current / maximum, 0, 1) * 100:0}%";
    }

    private static void ConfigureComboBox(ComboBox comboBox)
    {
        comboBox.Width = 260;
        comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBox.BackColor = Palette.Panel;
        comboBox.ForeColor = Palette.Text;
        comboBox.FlatStyle = FlatStyle.Flat;
        comboBox.Margin = new Padding(8, 6, 0, 0);
    }

    private static void ConfigureButton(Button button, string text)
    {
        button.Text = text;
        button.Width = 84;
        button.Height = 34;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = Palette.Accent;
        button.ForeColor = Color.White;
        button.Margin = new Padding(8, 4, 0, 0);
    }

    private void ConfigureSpeedSelector()
    {
        _speedSelector.Width = 72;
        _speedSelector.Minimum = 1;
        _speedSelector.Maximum = 5;
        _speedSelector.Value = 1;
        _speedSelector.BackColor = Palette.Panel;
        _speedSelector.ForeColor = Palette.Text;
        _speedSelector.BorderStyle = BorderStyle.None;
        _speedSelector.Margin = new Padding(8, 9, 0, 0);
    }

    private static class Palette
    {
        public static readonly Color Background = Color.FromArgb(12, 18, 24);
        public static readonly Color Panel = Color.FromArgb(24, 32, 40);
        public static readonly Color PanelAlt = Color.FromArgb(30, 42, 52);
        public static readonly Color Text = Color.FromArgb(230, 237, 243);
        public static readonly Color Muted = Color.FromArgb(142, 159, 171);
        public static readonly Color Accent = Color.FromArgb(44, 128, 154);
    }
}
