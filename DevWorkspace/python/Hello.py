import tkinter as tk

class MainForm(tk.Tk):
    def __init__(self):
        super().__init__()
        self.title("Hello WinForms - Python")
        self.geometry("320x180")

        self.lbl = tk.Label(self, text="", font=("Segoe UI", 11))
        self.lbl.place(x=20, y=20)

        btn = tk.Button(
            self,
            text="Say Hello",
            command=self.on_click,
            width=14
        )
        btn.place(x=20, y=60)

    def on_click(self):
        self.lbl.config(text="Hello, World!")

if __name__ == "__main__":
    app = MainForm()
    app.mainloop()