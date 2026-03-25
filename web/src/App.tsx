import { OrdersPage } from './pages/OrdersPage'

export default function App() {
  return (
    <div className="min-h-screen bg-gray-50">
      <header className="border-b border-gray-200 bg-white shadow-sm">
        <div className="mx-auto max-w-5xl px-4 py-4 flex items-center gap-3">
          <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-violet-600 text-white text-sm font-bold">
            O
          </div>
          <h1 className="text-base font-semibold text-gray-800">Orders Manager</h1>
        </div>
      </header>
      <main className="mx-auto max-w-5xl px-4 py-6">
        <OrdersPage />
      </main>
    </div>
  )
}
