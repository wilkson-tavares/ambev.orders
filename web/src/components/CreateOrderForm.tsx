import { useForm, useFieldArray } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { ordersApi } from '../api/orders'
import { Plus, Trash2 } from 'lucide-react'

const schema = z.object({
  orderId: z.number().int().positive('ID do pedido deve ser positivo'),
  clientId: z.number().int().positive('ID do cliente deve ser positivo'),
  items: z
    .array(
      z.object({
        productId: z.number().int().positive('ID do produto inválido'),
        quantity: z.number().int().min(1, 'Mínimo 1'),
        value: z.number().positive('Valor deve ser positivo'),
      }),
    )
    .min(1, 'Adicione ao menos um item'),
})

type FormData = z.infer<typeof schema>

interface Props {
  onSuccess: (id: number) => void
}

export function CreateOrderForm({ onSuccess }: Props) {
  const queryClient = useQueryClient()

  const {
    register,
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { items: [{ productId: undefined, quantity: 1, value: undefined }] },
  })

  const { fields, append, remove } = useFieldArray({ control, name: 'items' })

  const mutation = useMutation({
    mutationFn: ordersApi.create,
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['orders'] })
      reset()
      onSuccess(data.id)
    },
  })

  const onSubmit = (data: FormData) => mutation.mutate(data)

  const fieldClass = (error?: { message?: string }) =>
    `w-full rounded border px-3 py-1.5 text-sm outline-none focus:ring-2 focus:ring-violet-500 ${
      error ? 'border-red-400' : 'border-gray-300'
    }`

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="mb-1 block text-sm font-medium text-gray-700">ID do Pedido</label>
          <input type="number" {...register('orderId', { valueAsNumber: true })} className={fieldClass(errors.orderId)} />
          {errors.orderId && <p className="mt-1 text-xs text-red-500">{errors.orderId.message}</p>}
        </div>
        <div>
          <label className="mb-1 block text-sm font-medium text-gray-700">ID do Cliente</label>
          <input type="number" {...register('clientId', { valueAsNumber: true })} className={fieldClass(errors.clientId)} />
          {errors.clientId && <p className="mt-1 text-xs text-red-500">{errors.clientId.message}</p>}
        </div>
      </div>

      <div>
        <div className="mb-2 flex items-center justify-between">
          <span className="text-sm font-medium text-gray-700">Itens</span>
          <button
            type="button"
            onClick={() => append({ productId: undefined as unknown as number, quantity: 1, value: undefined as unknown as number })}
            className="flex items-center gap-1 rounded border border-violet-500 px-2 py-1 text-xs text-violet-600 hover:bg-violet-50"
          >
            <Plus size={12} /> Adicionar item
          </button>
        </div>

        {errors.items?.root && (
          <p className="mb-2 text-xs text-red-500">{errors.items.root.message}</p>
        )}

        <div className="space-y-2">
          {fields.map((field, index) => (
            <div key={field.id} className="flex items-start gap-2 rounded border border-gray-200 p-2">
              <div className="flex-1">
                <label className="mb-0.5 block text-xs text-gray-500">ID Produto</label>
                <input
                  type="number"
                  {...register(`items.${index}.productId`, { valueAsNumber: true })}
                  className={fieldClass(errors.items?.[index]?.productId)}
                />
                {errors.items?.[index]?.productId && (
                  <p className="mt-0.5 text-xs text-red-500">{errors.items[index]!.productId!.message}</p>
                )}
              </div>
              <div className="w-24">
                <label className="mb-0.5 block text-xs text-gray-500">Qtd</label>
                <input
                  type="number"
                  {...register(`items.${index}.quantity`, { valueAsNumber: true })}
                  className={fieldClass(errors.items?.[index]?.quantity)}
                />
              </div>
              <div className="w-32">
                <label className="mb-0.5 block text-xs text-gray-500">Valor (R$)</label>
                <input
                  type="number"
                  step="0.01"
                  {...register(`items.${index}.value`, { valueAsNumber: true })}
                  className={fieldClass(errors.items?.[index]?.value)}
                />
              </div>
              <button
                type="button"
                onClick={() => remove(index)}
                disabled={fields.length === 1}
                className="mt-5 text-gray-400 hover:text-red-500 disabled:opacity-30"
              >
                <Trash2 size={16} />
              </button>
            </div>
          ))}
        </div>
      </div>

      {mutation.isError && (
        <p className="rounded bg-red-50 px-3 py-2 text-sm text-red-600">
          {(mutation.error as { response?: { data?: { error?: string } } })?.response?.data?.error ?? 'Erro ao criar pedido'}
        </p>
      )}

      <button
        type="submit"
        disabled={mutation.isPending}
        className="w-full rounded bg-violet-600 py-2 text-sm font-medium text-white hover:bg-violet-700 disabled:opacity-60"
      >
        {mutation.isPending ? 'Criando...' : 'Criar Pedido'}
      </button>
    </form>
  )
}
