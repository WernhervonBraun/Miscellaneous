# Miscellaneous
Разные полезности для NetCF 3.5 одним репозиторием

Класс **Task** - аналог класса Task из библиотеки TPL.

Пример:
```
var task = new Task(()=>{
  //Что-то делаем...
  }));
task.Start()
```
или
```
Task.Run(()=>{
  //Что-то делаем...
  }));
```  
или
```
var task = Task.RunNew(()=>{
  //Что-то делаем...
  }));
```

Класс **ControlRunUIExtention** - метод-расширение RunUIContext аналогичный одноименному в Android SDK (Activity.ControlRunUIExtention)

Пример:
````
Form1.RunUIContext(()=>{
  //Код размещенный тут будет выполнет в потоке формы
}));
```
