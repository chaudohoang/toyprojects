import sys
import os
import json
import subprocess
import tempfile
from PyQt5.QtWidgets import (
    QApplication, QWidget, QVBoxLayout, QHBoxLayout, QPushButton, QListWidget,
    QLineEdit, QFileDialog, QMessageBox, QInputDialog
)

if getattr(sys, 'frozen', False):
    base_path = sys._MEIPASS
else:
    base_path = os.path.dirname(os.path.abspath(__file__))

AHK_PATH = os.path.join(base_path, 'autohotkey', 'AutoHotkeyU64.exe')
AHK2EXE_PATH = os.path.join(base_path, 'autohotkey', 'Ahk2Exe.exe')
UPX_PATH = os.path.join(base_path, 'autohotkey', 'upx.exe')

class RemapGUI(QWidget):
    def __init__(self):
        super().__init__()
        self.setWindowTitle("Remapper GUI")
        self.resize(600, 500)

        layout = QVBoxLayout(self)

        self.exe_field = QLineEdit()
        self.exe_field.setPlaceholderText("Target EXE:")
        layout.addWidget(self.exe_field)

        self.toggle_field = QLineEdit()
        self.toggle_field.setPlaceholderText("Toggle Key:")
        layout.addWidget(self.toggle_field)

        self.mapping_list = QListWidget()
        layout.addWidget(self.mapping_list)

        # Compact Button Grid
        btn_layout = QHBoxLayout()
        for label, method in [
            ("Clear Mapping", self.clear_mapping),
            ("Add Mapping", self.add_mapping),
            ("Edit Mapping", self.edit_mapping),
            ("Remove Mapping", self.remove_mapping),
        ]:
            b = QPushButton(label)
            b.clicked.connect(method)
            btn_layout.addWidget(b)
        layout.addLayout(btn_layout)

        json_layout = QHBoxLayout()
        for label, method in [
            ("Load JSON", self.load_json),
            ("Save JSON", self.save_json),
            ("Generate AHK", self.generate_ahk),
            ("Compile AHK to EXE", self.compile_ahk),
        ]:
            b = QPushButton(label)
            b.clicked.connect(method)
            json_layout.addWidget(b)
        layout.addLayout(json_layout)

        run_layout = QHBoxLayout()
        self.run_btn = QPushButton("Run Remap")
        self.run_btn.clicked.connect(self.run_remap)
        self.stop_btn = QPushButton("Stop Remap")
        self.stop_btn.clicked.connect(self.stop_remap)
        run_layout.addWidget(self.run_btn)
        run_layout.addWidget(self.stop_btn)
        layout.addLayout(run_layout)

        self.remap_process = None

    def clear_mapping(self):
        self.mapping_list.clear()

    def add_mapping(self):
        from_key, ok = QInputDialog.getText(self, "From Key", "Enter From Key:")
        if not ok or not from_key:
            return
        to_key, ok = QInputDialog.getText(self, "To Key", "Enter To Key:")
        if not ok or not to_key:
            return
        self.mapping_list.addItem(f"{from_key} -> {to_key}")

    def edit_mapping(self):
        current = self.mapping_list.currentRow()
        if current < 0:
            return
        item_text = self.mapping_list.item(current).text()
        from_key, to_key = item_text.split(" -> ")
        new_from, ok = QInputDialog.getText(self, "Edit From Key", "From Key:", text=from_key)
        if not ok:
            return
        new_to, ok = QInputDialog.getText(self, "Edit To Key", "To Key:", text=to_key)
        if not ok:
            return
        self.mapping_list.item(current).setText(f"{new_from} -> {new_to}")

    def remove_mapping(self):
        row = self.mapping_list.currentRow()
        if row >= 0:
            self.mapping_list.takeItem(row)

    def load_json(self):
        file, _ = QFileDialog.getOpenFileName(self, "Load JSON", "", "JSON Files (*.json)")
        if not file:
            return
        try:
            with open(file, 'r') as f:
                data = json.load(f)
            self.exe_field.setText(data.get("target_exe", ""))
            self.toggle_field.setText(data.get("toggle_key", ""))
            self.mapping_list.clear()
            remaps = data.get("remaps", {})
            for from_key, to_key in remaps.items():
                self.mapping_list.addItem(f"{from_key} -> {to_key}")
            QMessageBox.information(self, "Loaded", f"✅ Loaded {len(remaps)} remaps")
        except Exception as e:
            QMessageBox.warning(self, "Error", f"Failed to load JSON:\n{e}")

    def save_json(self):
        file, _ = QFileDialog.getSaveFileName(self, "Save JSON", "", "JSON Files (*.json)")
        if not file:
            return
        remaps = {}
        for i in range(self.mapping_list.count()):
            from_key, to_key = self.mapping_list.item(i).text().split(" -> ")
            remaps[from_key] = to_key
        data = {
            "target_exe": self.exe_field.text(),
            "toggle_key": self.toggle_field.text(),
            "remaps": remaps
        }
        with open(file, 'w') as f:
            json.dump(data, f, indent=4)
        QMessageBox.information(self, "Saved", "✅ JSON saved")

    def generate_ahk_script(self):
        remaps = []
        for i in range(self.mapping_list.count()):
            from_key, to_key = self.mapping_list.item(i).text().split(" -> ")
            remaps.append((from_key.strip(), to_key.strip()))

        target_exe = os.path.basename(self.exe_field.text().strip())
        toggle_key = self.toggle_field.text().strip() if self.toggle_field.text().strip() else "`"

        ahk_lines = [
            "#NoEnv",
            "#SingleInstance Force",
            "SetWorkingDir %A_ScriptDir%",
            "",
            "toggle := true  ; Default: Remap ON when started",
            ""
        ]

        # Toggle block - Game-specific or Global
        if target_exe:
            ahk_lines += [
                "; --- Toggle works ONLY inside the game ---",
                f"#IfWinActive ahk_exe {target_exe}",
                f"~{toggle_key}::",
                "toggle := !toggle",
                'SoundBeep, % (toggle ? 750 : 400), 150  ; Beep high if ON, low if OFF',
                "return",
                ""
            ]
        else:
            ahk_lines += [
                "; --- Global Toggle ---",
                f"~{toggle_key}::",
                "toggle := !toggle",
                'SoundBeep, % (toggle ? 750 : 400), 150  ; Beep high if ON, low if OFF',
                "return",
                ""
            ]

        # Remap block - Game-specific or Global
        if target_exe:
            ahk_lines.append(f'#If (toggle && WinActive("ahk_exe {target_exe}"))')
        else:
            ahk_lines.append(f'#If (toggle)  ; Global remap when toggle is ON')

        for from_key, to_key in remaps:
            ahk_lines.append(f"{from_key}::{to_key}")

        ahk_lines.append("")
        ahk_lines.append("#If")  # Reset context at the end

        return "\n".join(ahk_lines)


    def generate_ahk(self):
        file, _ = QFileDialog.getSaveFileName(self, "Save AHK", "remap.ahk", "AHK Files (*.ahk)")
        if not file:
            return
        script = self.generate_ahk_script()
        with open(file, 'w') as f:
            f.write(script)
        QMessageBox.information(self, "Generated", f"✅ AHK saved to {file}")

    def compile_ahk(self):
        with tempfile.NamedTemporaryFile(delete=False, suffix=".ahk") as tmp:
            tmp.write(self.generate_ahk_script().encode())
            tmp_ahk = tmp.name

        output_exe, _ = QFileDialog.getSaveFileName(self, "Save EXE", "remap_compiled.exe", "Executable Files (*.exe)")
        if not output_exe:
            os.unlink(tmp_ahk)
            return

        subprocess.run([AHK2EXE_PATH, '/in', tmp_ahk, '/out', output_exe])
        if os.path.exists(UPX_PATH):
            subprocess.run([UPX_PATH, '--best', '--lzma', output_exe])
        os.unlink(tmp_ahk)
        QMessageBox.information(self, "Compiled", f"✅ Compiled to {output_exe}")

    def run_remap(self):
        script_path = os.path.join(tempfile.gettempdir(), "remap.ahk")
        with open(script_path, 'w') as f:
            f.write(self.generate_ahk_script())
        if self.remap_process:
            self.remap_process.kill()
        self.remap_process = subprocess.Popen([AHK_PATH, script_path])
        QMessageBox.information(self, "Running", "✅ Remap is running!")

    def stop_remap(self):
        if self.remap_process:
            self.remap_process.kill()
            self.remap_process = None
            QMessageBox.information(self, "Stopped", "✅ Remap stopped")

if __name__ == "__main__":
    app = QApplication(sys.argv)
    window = RemapGUI()
    window.show()
    sys.exit(app.exec_())
