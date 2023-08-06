# AuthService - сервис авторизации с refresh и access JWT-токенами

## Стэк
C#, Asp.Net Core 7, Entity Framework Core.  
REST API

# Развёртывание
Для развёртывания на локальной машине требуется создать в папке API файл security.json с настройками JWT токенов и сроки подключения: 
```
{
    "Issuer" : "Issuer",
    "Audience" : "Audience",
    "Secret" : "secretKey",
    "TimeLife" : "1",
    "ConnectionStrings": {
        "DefaultConnection": "по умолчанию MSSQL Server;"
  }
}
```

После создания security.json запустите проект в папке \API  командой : 
```dotnet run```
Проект запущен.

# Тесты
В проекте я использовал XUnit для тестирования фукциональности.
### Создатель проекта обучался тестированию, поэтому тесты не охватывают всю функциональность сервиса.

Для запуска тестов требуется в папке \Tests выполнить команду ```dotnet test```
