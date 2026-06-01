# Blazor Auth Practice

Учебный проект: связка **Blazor WebAssembly** и **ASP.NET Core Web API** с JWT-авторизацией и ролевым доступом.

## Что умеет

- Регистрация и вход — API выдаёт JWT-токен, Blazor сохраняет его в сессии
- Ролевая модель (пользователь / администратор) — кастомный атрибут `[RoleAuthorize]`
- Каталог фильмов: просмотр, создание, редактирование (только для администратора)
- Чат между пользователями
- Загрузка и просмотр изображений

## Стек

| Слой | Технология |
|------|------------|
| Бэкенд | ASP.NET Core 8 Web API |
| Фронтенд | Blazor WebAssembly (.NET 8) |
| Авторизация | JWT + кастомный `[RoleAuthorize]` |
| ORM | Entity Framework Core |

## Структура

```
BlazorPractic1/
├── AuthApi/           # ASP.NET Core Web API (JWT, роли, контроллеры)
└── BlazorPractice1/   # Blazor WebAssembly клиент (страницы, сервисы)
```

## Запуск

```bash
# 1. Запустить API
cd AuthApi/AuthApi
dotnet run

# 2. Запустить Blazor-клиент
cd BlazorPractice1/BlazorPractice1
dotnet run
```

После запуска:
- API: `https://localhost:7xxx`
- Клиент: `https://localhost:5xxx`